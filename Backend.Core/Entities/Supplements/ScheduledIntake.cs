using Backend.Core.Enums;

namespace Backend.Core.Entities.Supplements;

/// <summary>
/// Represents a supplement assigned to a specific chronobiology-based intake slot.
/// </summary>
public sealed record ScheduledIntake(SupplementProduct SupplementProduct, IntakeSlot Slot);
