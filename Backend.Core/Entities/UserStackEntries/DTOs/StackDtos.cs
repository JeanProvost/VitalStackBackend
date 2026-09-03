using Backend.Core.Enums;

namespace Backend.Core.Entities.UserStackEntries.DTOs;

public sealed record AddToStackResponseDto(
    string UserId,
    int SupplementProductId,
    StackSupplementProductDto SupplementProduct,
    string? CustomName,
    StackCustomizationDto Cusomization,
    ScheduleTimeBlock IntendedTime,
    string? ContextualInstruction,
    decimal ServingMultiplier,
    bool IsActive,
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record StackSupplementProductDto(
    string DsldId,
    string ProductName,
    string? BrandName);

public sealed record StackCustomizationDto(
    string? Form,
    string? Dosage,
    string? Brand,
    string? TimeOfDayTarget);
