using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Entities.UserStackEntries.DTOs;
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

        if (request.SupplementProductId == null)
        {
            return Results.BadRequest("SupplementProductId must be greater than zero.");
        }

        if (request.ServingMultiplier is <= 0 or > StackService.MaximumServingMultiplier)
        {
            return Results.BadRequest(
                $"ServingMultiplier must be greater than zero and no more than {StackService.MaximumServingMultiplier}.");
        }

        var (newEntry, isDuplicate) = await stackService.CreateEntryAsync(
            request,
            userId,
            db.SupplementProducts,
            db.UserStackEntries,
            cancellationToken);

        if (isDuplicate)
        {
            return Results.Conflict(
                $"Supplement product {request.SupplementProductId} is already in the user's stack.");
        }

        if (newEntry is null)
        {
            return Results.NotFound($"Supplement product {request.SupplementProductId} was not found.");
        }

        db.UserStackEntries.Add(newEntry);
        await db.SaveChangesAsync(cancellationToken);

        var supplementProductId = newEntry.SupplementProductId
            ?? throw new InvalidOperationException("A catalog stack entry must have a supplement product ID.");
        var supplementProduct = await stackService.GetProductSummaryAsync(
            supplementProductId,
            db.SupplementProducts,
            cancellationToken);

        var response = new AddToStackResponseDto(
            newEntry.UserId,
            supplementProductId,
            supplementProduct,
            newEntry.CustomName,
            new StackCustomizationDto(
                newEntry.Cusomization.Form,
                newEntry.Cusomization.Dosage,
                newEntry.Cusomization.Brand,
                newEntry.Cusomization.TimeOfDayTarget),
            newEntry.IntendedTime,
            newEntry.ContextualInstruction,
            newEntry.ServingMultiplier,
            newEntry.IsActive,
            newEntry.Id,
            newEntry.CreatedAt,
            newEntry.UpdatedAt);

        return Results.Created($"/api/stack/{newEntry.Id}", response);
    }
}
