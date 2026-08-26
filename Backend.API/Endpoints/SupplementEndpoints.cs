using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Services;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

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
