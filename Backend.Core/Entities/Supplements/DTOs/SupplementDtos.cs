using Backend.Core.Enums;

namespace Backend.Core.Entities.Supplements.DTOs;

public record SupplementAutocompleteSuggestionDto(
    int Id,
    string DsldId,
    string ProductName,
    string? BrandName,
    string? ThumbnailUrl,
    string? LabelPdfUrl
);

public record SupplementSearchResultDto(
    int Id,
    string DsldId,
    string ProductName,
    string? BrandName,
    string Form,
    string? ThumbnailUrl,
    string? LabelPdfUrl,
    List<IngredientSummaryDto> Ingredients
);

public record IngredientSummaryDto(
    string Name,
    decimal DosageAmount,
    string DosageUnit
);

public record AddToStackRequest(
    string SupplementProductId,
    decimal ServingMultiplier = 1.0m,
    ScheduleTimeBlock? IntendedTime = null,
    string? ContextualInstruction = null
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
