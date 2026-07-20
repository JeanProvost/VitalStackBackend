using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Entities.UserStackEntries;
using Backend.Core.Enums;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Backend.API.Endpoints
{
    public static class SupplementEndpoints
    {
        public static RouteGroupBuilder MapSupplementEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/autocomplete", AutocompleteAsync);

            group.MapGet("/search", async (
                [FromQuery] string query,
                ApplicationDbContext _db) =>
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Results.BadRequest("Query is empty");
                }

                var searchPattern = $"%{query.Trim()}%";

                const int resultLimit = 25;

                // Product-name matches have absolute priority. Fetching these first avoids calculating
                // ingredient percentages for thousands of products that cannot reach the response.
                var rankedProductIds = await _db.SupplementProducts
                    .AsNoTracking()
                    .Where(p => EF.Functions.ILike(p.ProductName, searchPattern))
                    .OrderBy(p => p.ProductName)
                    .ThenBy(p => p.Id)
                    .Select(p => p.Id)
                    .Take(resultLimit)
                    .ToListAsync();

                var remaining = resultLimit - rankedProductIds.Count;
                if (remaining > 0)
                {
                    var ingredientProductIds = await _db.SupplementProducts
                        .AsNoTracking()
                        .Where(p => !rankedProductIds.Contains(p.Id) &&
                            p.ActiveIngredients.Any(pi =>
                                EF.Functions.ILike(pi.Ingredient.CanonicalName, searchPattern)))
                        .Select(p => new
                        {
                            p.Id,
                            p.ProductName,
                            // Convert mass units to mg so a 1 g ingredient is not ranked below 500 mg.
                            MatchingIngredientMass = p.ActiveIngredients
                                .Where(pi => EF.Functions.ILike(pi.Ingredient.CanonicalName, searchPattern))
                                .Sum(pi => pi.DosageUnit.ToLower() == "g" ? pi.DosageAmount * 1000m :
                                    pi.DosageUnit.ToLower() == "mg" ? pi.DosageAmount :
                                    pi.DosageUnit.ToLower() == "mcg" ? pi.DosageAmount / 1000m : 0m),
                            TotalIngredientMass = p.ActiveIngredients
                                .Sum(pi => pi.DosageUnit.ToLower() == "g" ? pi.DosageAmount * 1000m :
                                    pi.DosageUnit.ToLower() == "mg" ? pi.DosageAmount :
                                    pi.DosageUnit.ToLower() == "mcg" ? pi.DosageAmount / 1000m : 0m)
                        })
                        .OrderByDescending(x => x.TotalIngredientMass == 0m
                            ? 0m
                            : x.MatchingIngredientMass / x.TotalIngredientMass)
                        .ThenBy(x => x.ProductName)
                        .ThenBy(x => x.Id)
                        .Select(x => x.Id)
                        .Take(remaining)
                        .ToListAsync();

                    rankedProductIds.AddRange(ingredientProductIds);
                    remaining -= ingredientProductIds.Count;
                }

                if (remaining > 0)
                {
                    var brandProductIds = await _db.SupplementProducts
                        .AsNoTracking()
                        .Where(p => !rankedProductIds.Contains(p.Id) &&
                            p.BrandName != null &&
                            EF.Functions.ILike(p.BrandName, searchPattern))
                        .OrderBy(p => p.ProductName)
                        .ThenBy(p => p.Id)
                        .Select(p => p.Id)
                        .Take(remaining)
                        .ToListAsync();

                    rankedProductIds.AddRange(brandProductIds);
                }

                var productResults = await _db.SupplementProducts
                    .AsNoTracking()
                    .Where(p => rankedProductIds.Contains(p.Id))
                    .Select(p => new SupplementSearchResultDto(
                        p.Id,
                        p.ProductName,
                        p.BrandName,
                        p.Form,
                        p.ActiveIngredients.Select(pi => new IngredientSummaryDto(
                            pi.Ingredient.CanonicalName,
                            pi.DosageAmount,
                            pi.DosageUnit
                        )).ToList()
                    ))
                    .ToListAsync();

                var resultsByProductId = productResults.ToDictionary(result => result.Id);
                var results = rankedProductIds.Select(id => resultsByProductId[id]).ToList();

                return Results.Ok(results);
            });

            group.MapPost("/stack", async (
                [FromBody] AddToStackRequest request,
                ApplicationDbContext _db,
                ClaimsPrincipal user,
                HttpClient httpClient,
                IConfiguration config,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Results.Unauthorized();
                }

                if (request.SupplementProductId == null && string.IsNullOrWhiteSpace(request.CustomName))
                {
                    return Results.BadRequest("CustomName or SupplementProductId required");
                }

                var nowUtc = DateTime.UtcNow;
                var optimization = await OptimizeScheduleAsync(
                    request,
                    _db,
                    httpClient,
                    config,
                    cancellationToken);

                var intendedTime = request.IntendedTime
                    ?? optimization.IntendedTime
                    ?? ResolveLegacyTimeTarget(request.TimeOfDayTarget)
                    ?? ScheduleTimeBlock.Morning;

                var newEntry = new UserStackEntry
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    SupplementProductId = request.SupplementProductId,
                    CustomName = request.CustomName,
                    IntendedTime = intendedTime,
                    ContextualInstruction = request.ContextualInstruction ?? optimization.ContextualInstruction,
                    CreatedAt = nowUtc,
                    UpdatedAt = nowUtc,
                    Cusomization = new StackCustomization
                    {
                        Form = request.Form,
                        Dosage = request.Dosage,
                        Brand = request.Brand,
                        TimeOfDayTarget = request.TimeOfDayTarget ?? intendedTime.ToString()
                    }
                };

                _db.UserStackEntries.Add(newEntry);
                await _db.SaveChangesAsync();

                return Results.Created($"/api/supplements/stack/{newEntry.Id}", newEntry);
            });

            return group;
        }

        public static async Task<IResult> AutocompleteAsync(
            [FromQuery] string? query,
            ApplicationDbContext db,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.BadRequest("Query is empty");
            }

            var trimmedQuery = query.Trim();
            if (trimmedQuery.Length < SupplementAutocompleteQuery.MinimumQueryLength)
            {
                return Results.Ok(Array.Empty<SupplementAutocompleteSuggestionDto>());
            }

            var suggestions = await SupplementAutocompleteQuery
                .Create(db.SupplementProducts, trimmedQuery)
                .ToListAsync(cancellationToken);

            return Results.Ok(suggestions);
        }

        private static async Task<ScheduleOptimization> OptimizeScheduleAsync(
            AddToStackRequest request,
            ApplicationDbContext db,
            HttpClient httpClient,
            IConfiguration config,
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

            SupplementProduct? product = null;
            if (request.SupplementProductId is not null)
            {
                product = await db.SupplementProducts
                    .Include(p => p.ActiveIngredients)
                        .ThenInclude(pi => pi.Ingredient)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.SupplementProductId, cancellationToken);
            }

            var primaryIngredient = product?.ActiveIngredients.FirstOrDefault();
            var supplementName = primaryIngredient?.Ingredient.CanonicalName
                ?? product?.ProductName
                ?? request.CustomName;

            if (string.IsNullOrWhiteSpace(supplementName))
            {
                return new ScheduleOptimization(null, null);
            }

            var dosage = request.Dosage ?? (primaryIngredient is null
                ? null
                : $"{primaryIngredient.DosageAmount} {primaryIngredient.DosageUnit}");

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

                var response = await httpClient.PostAsJsonAsync(
                    endpoint,
                    new OptimizeScheduleRequest(
                        supplementName,
                        request.Form ?? product?.Form,
                        dosage,
                        request.Brand ?? product?.BrandName,
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

        private static ScheduleTimeBlock? ResolveLegacyTimeTarget(string? timeOfDayTarget)
        {
            if (string.IsNullOrWhiteSpace(timeOfDayTarget))
            {
                return null;
            }

            if (Enum.TryParse<ScheduleTimeBlock>(timeOfDayTarget, ignoreCase: true, out var block))
            {
                return block;
            }

            return timeOfDayTarget.Trim().ToLowerInvariant() switch
            {
                "uponwaking" or "withbreakfast" or "midmorning" => ScheduleTimeBlock.Morning,
                "withlunch" or "midafternoon" => ScheduleTimeBlock.Afternoon,
                "withdinner" or "evening" => ScheduleTimeBlock.Evening,
                "beforebed" => ScheduleTimeBlock.Night,
                _ => null
            };
        }

        private static ScheduleTimeBlock? ParseTimeBlock(string? timeOfDay)
        {
            return string.IsNullOrWhiteSpace(timeOfDay)
                ? null
                : ResolveLegacyTimeTarget(timeOfDay);
        }

    }
}
