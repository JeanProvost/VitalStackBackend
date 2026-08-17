using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Entities.UserStackEntries;
using Backend.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Backend.Core.Services;

public class StackService(
    HttpClient httpClient,
    IConfiguration config)
{
    public const decimal MaximumServingMultiplier = 999.99m;

    public async Task<UserStackEntry?> CreateEntryAsync(
        AddToStackRequest request,
        string userId,
        IQueryable<SupplementProduct> supplementProducts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(supplementProducts);

        var productQuery = supplementProducts
            .Include(product => product.ActiveIngredients)
                .ThenInclude(productIngredient => productIngredient.Ingredient)
            .AsNoTracking()
            .Where(product => product.Id == request.SupplementProductId);

        var product = productQuery.Provider is IAsyncQueryProvider
            ? await productQuery.FirstOrDefaultAsync(cancellationToken)
            : productQuery.FirstOrDefault();

        if (product is null)
        {
            return null;
        }

        var optimization = await OptimizeScheduleAsync(request, product, cancellationToken);
        var intendedTime = request.IntendedTime
            ?? optimization.IntendedTime
            ?? ScheduleTimeBlock.Morning;
        var nowUtc = DateTime.UtcNow;

        return new UserStackEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SupplementProductId = product.Id,
            IntendedTime = intendedTime,
            ContextualInstruction = request.ContextualInstruction ?? optimization.ContextualInstruction,
            ServingMultiplier = request.ServingMultiplier,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };
    }

    private async Task<ScheduleOptimization> OptimizeScheduleAsync(
        AddToStackRequest request,
        SupplementProduct product,
        CancellationToken cancellationToken)
    {
        if (request.IntendedTime is not null && !string.IsNullOrWhiteSpace(request.ContextualInstruction))
        {
            return new ScheduleOptimization(null, null);
        }

        var baseUrl = config["AiService:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl)
            || !Uri.TryCreate(baseUrl.TrimEnd('/') + "/optimize-schedule", UriKind.Absolute, out var endpoint))
        {
            return new ScheduleOptimization(null, null);
        }

        var primaryIngredient = product.ActiveIngredients.FirstOrDefault();
        var supplementName = primaryIngredient?.Ingredient.CanonicalName
            ?? product.ProductName;
        var dosage = primaryIngredient is null
            ? null
            : $"{primaryIngredient.DosageAmount} {primaryIngredient.DosageUnit}";

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

            var response = await httpClient.PostAsJsonAsync(
                endpoint,
                new OptimizeScheduleRequest(
                    supplementName,
                    product.Form,
                    dosage,
                    product.BrandName,
                    null),
                linkedToken.Token);

            if (!response.IsSuccessStatusCode)
            {
                return new ScheduleOptimization(null, null);
            }

            var optimized = await response.Content.ReadFromJsonAsync<OptimizeScheduleResponse>(
                cancellationToken: linkedToken.Token);

            return new ScheduleOptimization(
                ParseTimeBlock(optimized?.TimeOfDay),
                optimized?.ContextualInstruction);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ScheduleOptimization(null, null);
        }
        catch (HttpRequestException)
        {
            return new ScheduleOptimization(null, null);
        }
    }

    private static ScheduleTimeBlock? ParseTimeBlock(string? timeOfDay)
    {
        if (string.IsNullOrWhiteSpace(timeOfDay))
        {
            return null;
        }

        if (Enum.TryParse<ScheduleTimeBlock>(timeOfDay, ignoreCase: true, out var block))
        {
            return block;
        }

        return timeOfDay.Trim().ToLowerInvariant() switch
        {
            "uponwaking" or "withbreakfast" or "midmorning" => ScheduleTimeBlock.Morning,
            "withlunch" or "midafternoon" => ScheduleTimeBlock.Afternoon,
            "withdinner" or "evening" => ScheduleTimeBlock.Evening,
            "beforebed" => ScheduleTimeBlock.Night,
            _ => null
        };
    }
}
