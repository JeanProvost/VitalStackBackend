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
            //Search Supplement catalog
            group.MapGet("/search", async (
                [FromQuery] string query,
                ApplicationDbContext _db) =>
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Results.BadRequest("Query is empty");
                }

                var results = await _db.Supplements
                    .Where(s => EF.Functions.ILike(s.Name, $"%{query}%") ||
                        s.Aliases.Any(a => EF.Functions.ILike(a, $"%{query}%")))
                    .Take(25)
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(results);
            });

            //Add Supplement to User Stack
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

                if (request.MasterSupplementId == null && string.IsNullOrWhiteSpace(request.CustomName))
                {
                    return Results.BadRequest("CustomerName and MasterSupplementId required");
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
                    MasterSupplementId = request.MasterSupplementId,
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

            var supplement = request.MasterSupplementId is null
                ? null
                : await db.Supplements
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.MasterSupplementId, cancellationToken);

            var supplementName = supplement?.Name ?? request.CustomName;
            if (string.IsNullOrWhiteSpace(supplementName))
            {
                return new ScheduleOptimization(null, null);
            }

            var dosage = request.Dosage ?? (supplement is null
                ? null
                : $"{supplement.DosageAmount} {supplement.DosageUnit}");

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

                var response = await httpClient.PostAsJsonAsync(
                    endpoint,
                    new OptimizeScheduleRequest(
                        supplementName,
                        request.Form ?? supplement?.Form,
                        dosage,
                        request.Brand ?? supplement?.Brand,
                        supplement?.RequiresFood),
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
