using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Endpoints
{
    public static class SupplementEndpoints
    {
        public static RouteGroupBuilder MapSupplementEndpoints(this RouteGroupBuilder group)
        {
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

            return group;
        }
    }
}
