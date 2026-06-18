using Backend.Core.Enums;

namespace Backend.API.Endpoints;

public record DailyScheduleResponse(
    DateOnly Date,
    string TimeZone,
    IReadOnlyList<ScheduleBlockResponse> Blocks);

public record ScheduleBlockResponse(
    ScheduleTimeBlock TimeOfDay,
    IReadOnlyList<ScheduleItemResponse> Items);

public record ScheduleItemResponse(
    Guid UserStackEntryId,
    Guid? MasterSupplementId,
    string Name,
    string? Form,
    string? Dosage,
    string? Brand,
    ScheduleTimeBlock IntendedTime,
    string? ContextualInstruction,
    bool IsLoggedToday);

public record LogScheduleRequest(
    IReadOnlyCollection<Guid> UserStackEntryIds,
    DateTimeOffset? TakenAt);

public record LogScheduleResponse(
    DateTime TakenAtUtc,
    IReadOnlyList<Guid> IntakeLogIds);
