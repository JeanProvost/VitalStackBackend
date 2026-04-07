using Backend.Core.Entities.Base;

namespace Backend.Core.Entities.Supplement;

/// <summary>
/// Represents a supplement item that belongs to a user's supplement stack.
/// This captures supplement-specific dosage and timing metadata consumed by interaction
/// and chronobiology logic.
/// </summary>
public class Supplement : BaseEntity<Guid>
{
        public required Guid SupplementStackId { get; set; }

    
    public global::Backend.Core.Entities.SupplementStack.SupplementStack? SupplementStack { get; set; }

    public required Guid SupplementId { get; set; }
    public string? Brand { get; set; }
    public required string Form { get; set; }
    public required decimal DosageAmount { get; set; }
    public required string DosageUnit { get; set; }
    public required string TimeOfDay { get; set; }
    public bool? RequiresFood { get; set; }
    public required bool IsActive { get; set; } = true;
}
