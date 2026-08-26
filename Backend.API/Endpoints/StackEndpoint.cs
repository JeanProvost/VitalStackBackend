using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Services;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.API.Endpoints;

public static class StackEndpoint
{
    public static RouteGroupBuilder MapStackEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("", AddSupplementAsync);

        return group;
    }

    public static async Task<IResult> AddSupplementAsync(
        [FromBody] AddToStackRequest request,
        ApplicationDbContext db,
        ClaimsPrincipal user,
        StackService stackService,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        if (request.SupplementProductId <= 0)
        {
            return Results.BadRequest("SupplementProductId must be greater than zero.");
        }

        if (request.ServingMultiplier is <= 0 or > StackService.MaximumServingMultiplier)
        {
            return Results.BadRequest(
                $"ServingMultiplier must be greater than zero and no more than {StackService.MaximumServingMultiplier}.");
        }

        var newEntry = await stackService.CreateEntryAsync(
            request,
            userId,
            db.SupplementProducts,
            cancellationToken);

        if (newEntry is null)
        {
            return Results.NotFound($"Supplement product {request.SupplementProductId} was not found.");
        }

        db.UserStackEntries.Add(newEntry);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/stack/{newEntry.Id}", newEntry);
    }
}
