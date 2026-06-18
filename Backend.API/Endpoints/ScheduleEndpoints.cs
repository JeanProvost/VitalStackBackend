using Backend.Core.Entities.IntakeLogs;
using Backend.Core.Entities.UserStackEntries;
using Backend.Core.Enums;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.API.Endpoints
{
    public static class ScheduleEndpoints
    {
        public static RouteGroupBuilder MapScheduleEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/today", GetToday)
                .WithName("GetTodaySchedule");

            group.MapPost("/log", LogIntake)
                .WithName("LogScheduleIntake");

            return group;
        }

        private static async Task<IResult> GetToday(
            ApplicationDbContext db,
            ClaimsPrincipal user,
            [FromQuery(Name = "timeZone")] string? timeZoneQuery,
            [FromHeader(Name = "X-Time-Zone")] string? timeZoneHeader)
        {
            var userId = GetUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var timeZone = ResolveTimeZone(timeZoneQuery ?? timeZoneHeader);
            var dateWindow = GetUtcDateWindow(timeZone);

            var stackEntries = await db.UserStackEntries
                .Include(x => x.MasterSupplement)
                .Where(x => x.UserId == userId && x.IsActive)
                .AsNoTracking()
                .ToListAsync();

            var loggedEntryIds = await db.IntakeLogs
                .Where(x => x.UserId == userId
                    && x.TakenAtUtc >= dateWindow.UtcStart
                    && x.TakenAtUtc < dateWindow.UtcEnd)
                .Select(x => x.UserStackEntryId)
                .Distinct()
                .ToListAsync();

            var loggedSet = loggedEntryIds.ToHashSet();
            var blocks = Enum.GetValues<ScheduleTimeBlock>()
                .Select(block => new ScheduleBlockResponse(
                    block,
                    stackEntries
                        .Where(x => x.IntendedTime == block)
                        .OrderBy(ResolveSupplementName)
                        .Select(x => MapScheduleItem(x, loggedSet.Contains(x.Id)))
                        .ToList()))
                .ToList();

            return Results.Ok(new DailyScheduleResponse(
                dateWindow.LocalDate,
                timeZone.Id,
                blocks));
        }

        private static async Task<IResult> LogIntake(
            [FromBody] LogScheduleRequest request,
            ApplicationDbContext db,
            ClaimsPrincipal user)
        {
            var userId = GetUserId(user);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (request.UserStackEntryIds is null || request.UserStackEntryIds.Count == 0)
            {
                return Results.BadRequest("At least one stack entry id is required.");
            }

            var requestedIds = request.UserStackEntryIds.Distinct().ToList();
            var stackEntries = await db.UserStackEntries
                .Include(x => x.MasterSupplement)
                .Where(x => requestedIds.Contains(x.Id) && x.UserId == userId && x.IsActive)
                .ToListAsync();

            if (stackEntries.Count != requestedIds.Count)
            {
                return Results.BadRequest("One or more stack entry ids are invalid for this user.");
            }

            var nowUtc = DateTime.UtcNow;
            var takenAtUtc = request.TakenAt?.UtcDateTime ?? nowUtc;

            var logs = stackEntries.Select(entry => new IntakeLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserStackEntryId = entry.Id,
                TakenAtUtc = takenAtUtc,
                IntendedTime = entry.IntendedTime,
                SupplementName = ResolveSupplementName(entry),
                Dosage = ResolveDosage(entry),
                ContextualInstruction = entry.ContextualInstruction,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc
            }).ToList();

            db.IntakeLogs.AddRange(logs);
            await db.SaveChangesAsync();

            return Results.Ok(new LogScheduleResponse(
                takenAtUtc,
                logs.Select(x => x.Id).ToList()));
        }

        private static ScheduleItemResponse MapScheduleItem(UserStackEntry entry, bool isLoggedToday)
        {
            return new ScheduleItemResponse(
                entry.Id,
                entry.MasterSupplementId,
                ResolveSupplementName(entry),
                entry.Cusomization.Form ?? entry.MasterSupplement?.Form,
                ResolveDosage(entry),
                entry.Cusomization.Brand ?? entry.MasterSupplement?.Brand,
                entry.IntendedTime,
                entry.ContextualInstruction,
                isLoggedToday);
        }

        private static string? ResolveDosage(UserStackEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(entry.Cusomization.Dosage))
            {
                return entry.Cusomization.Dosage;
            }

            return entry.MasterSupplement is null
                ? null
                : $"{entry.MasterSupplement.DosageAmount} {entry.MasterSupplement.DosageUnit}";
        }

        private static string ResolveSupplementName(UserStackEntry entry)
        {
            return entry.MasterSupplement?.Name
                ?? entry.CustomName
                ?? "Supplement";
        }

        private static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? user.FindFirstValue("username")
                ?? user.FindFirstValue("cognito:username");
        }

        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }

        private static (DateOnly LocalDate, DateTime UtcStart, DateTime UtcEnd) GetUtcDateWindow(TimeZoneInfo timeZone)
        {
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var localDate = DateOnly.FromDateTime(nowLocal);
            var localStart = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            var localEnd = localStart.AddDays(1);

            return (
                localDate,
                TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone),
                TimeZoneInfo.ConvertTimeToUtc(localEnd, timeZone));
        }
    }
}
