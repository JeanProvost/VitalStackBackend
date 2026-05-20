using Backend.Core.Entities.UserStackEntries;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
                ClaimsPrincipal user) =>
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (request.MasterSupplementId == null && string.IsNullOrWhiteSpace(request.CustomName))
                {
                    return Results.BadRequest("CustomerName and MasterSupplementId required");
                }

                var newEntry = new UserStackEntry
                {
                    UserId = userId,
                    MasterSupplementId = request.MasterSupplementId,
                    CustomName = request.CustomName,
                    Cusomization = new StackCustomization
                    {
                        Form = request.Form,
                        Dosage = request.Dosage,
                        Brand = request.Brand,
                        TimeOfDayTarget = request.TimeOfDayTarget
                    }
                };

                _db.UserStackEntries.Add(newEntry);
                await _db.SaveChangesAsync();

                return Results.Created($"/api/supplements/stack/{newEntry.Id}", newEntry);
            });

            return group;
        }

        public record AddToStackRequest(
            Guid? MasterSupplementId,
            string? CustomName,
            string? Form,
            string? Dosage,
            string? Brand,
            string? TimeOfDayTarget
        );
    }
}
