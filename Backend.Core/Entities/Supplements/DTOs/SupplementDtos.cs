using Backend.Core.Enums;

namespace Backend.Core.Entities.Supplements.DTOs;

public record SupplementAutocompleteSuggestionDto(
    int Id,
    string ProductName,
    string? BrandName
);

public record SupplementSearchResultDto(
    int Id,
    string ProductName,
    string? BrandName,
    string Form,
    List<IngredientSummaryDto> Ingredients
);

public record IngredientSummaryDto(
    string Name,
    decimal DosageAmount,
    string DosageUnit
);

public record AddToStackRequest(
    int? SupplementProductId,
    string? CustomName,
    string? Form,
    string? Dosage,
    string? Brand,
    string? TimeOfDayTarget,
    ScheduleTimeBlock? IntendedTime,
    string? ContextualInstruction
);

public record ScheduleOptimization(
    ScheduleTimeBlock? IntendedTime,
    string? ContextualInstruction);

public record OptimizeScheduleRequest(
    string SupplementName,
    string? Form,
    string? Dosage,
    string? Brand,
    bool? RequiresFood);

public record OptimizeScheduleResponse(
    string? TimeOfDay,
    string? ContextualInstruction);
