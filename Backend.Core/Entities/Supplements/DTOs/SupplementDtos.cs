using Backend.Core.Enums;

namespace Backend.Core.Entities.Supplements.DTOs;

public record AddToStackRequest(
    Guid? MasterSupplementId,
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
