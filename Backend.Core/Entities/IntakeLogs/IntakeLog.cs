using Backend.Core.Entities.Base;
using Backend.Core.Entities.UserStackEntries;
using Backend.Core.Enums;

namespace Backend.Core.Entities.IntakeLogs;

public class IntakeLog : BaseEntity<Guid>
{
    public required string UserId { get; set; }
    public Guid UserStackEntryId { get; set; }
    public UserStackEntry? UserStackEntry { get; set; }
    public DateTime TakenAtUtc { get; set; }
    public ScheduleTimeBlock IntendedTime { get; set; }
    public required string SupplementName { get; set; }
    public string? Dosage { get; set; }
    public string? ContextualInstruction { get; set; }
}
