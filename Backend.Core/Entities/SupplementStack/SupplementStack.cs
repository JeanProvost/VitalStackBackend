using Backend.Core.Entities.Base;
using Backend.Core.Enums;

namespace Backend.Core.Entities.SupplementStack;

/// <summary>
/// Represents a user's supplement stack container.
/// A stack can contain multiple supplement entries.
/// </summary>
public class SupplementStack : BaseEntity<Guid>
{
    public required string UserId { get; set; }
    public required Guid SupplementId { get; set; }
    public string? Brand { get; set; }

    //Chemical Form
    public required string Form { get; set; }
    public required decimal DosageAmount { get; set; }
    public required string DosageUnit { get; set; }

    //TTT
    public required string TimeToTake { get; set; }
    public SupplementTimeWindow TimeWindow
    {
        get => Enum.Parse<SupplementTimeWindow>(TimeToTake);
        set => TimeToTake = value.ToString();
    }

    public bool? RequiresFood { get; set; }
    public required bool IsActive { get; set; } = true;
}
