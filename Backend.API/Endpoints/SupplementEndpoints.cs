using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Services;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
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
                ApplicationDbContext db,
                SupplementService supplementService,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Results.BadRequest("Query is empty");
                }

                var results = await supplementService.SearchAsync(
                    db.SupplementProducts,
                    query,
                    cancellationToken);

                return Results.Ok(results);
            });

            group.MapPost("/stack", async (
                [FromBody] AddToStackRequest request,
                ApplicationDbContext db,
                ClaimsPrincipal user,
                SupplementService supplementService,
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

                var newEntry = await supplementService.CreateStackEntryAsync(
                    request,
                    userId,
                    db.SupplementProducts,
                    cancellationToken);

                db.UserStackEntries.Add(newEntry);
                await db.SaveChangesAsync(cancellationToken);

                return Results.Created($"/api/supplements/stack/{newEntry.Id}", newEntry);
            });

            return group;
        }

        public static async Task<IResult> AutocompleteAsync(
            [FromQuery] string? query,
            ApplicationDbContext db,
            SupplementService supplementService,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.BadRequest("Query is empty");
            }

            var suggestions = await supplementService.AutocompleteAsync(
                db.SupplementProducts,
                query,
                cancellationToken);

            return Results.Ok(suggestions);
        }

    }
}
