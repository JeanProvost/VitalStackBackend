using Backend.Core.Entities.Base;

namespace Backend.Core.Entities.Supplement;

/// <summary>
/// Represents a supplement item that belongs to a user's supplement stack.
/// This captures supplement-specific dosage and timing metadata consumed by interaction
/// and chronobiology logic.
/// </summary>
public class Supplement : BaseEntity<Guid>
{
    /// <summary>
    /// Gets or sets the parent supplement stack identifier.
    /// </summary>
    public required Guid SupplementStackId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the supplement from the master supplement catalog.
    /// </summary>
    public required Guid SupplementId { get; set; }

    /// <summary>
    /// Gets or sets the supplement manufacturer brand for product-level specificity.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the supplement form (for example capsule, powder, or liposomal),
    /// which is clinically relevant for bioavailability interpretation.
    /// </summary>
    public required string Form { get; set; }

    /// <summary>
    /// Gets or sets the numeric quantity taken per serving.
    /// </summary>
    public required decimal DosageAmount { get; set; }

    /// <summary>
    /// Gets or sets the dosage unit (for example mg, mcg, or IU) used by dose normalization logic.
    /// </summary>
    public required string DosageUnit { get; set; }

    /// <summary>
    /// Gets or sets the time-of-day window when the supplement is consumed.
    /// This is used by chronobiology and circadian scheduling logic.
    /// </summary>
    public required string TimeOfDay { get; set; }

    /// <summary>
    /// Gets or sets whether the supplement should be taken with food or in a fasted state.
    /// </summary>
    public bool? RequiresFood { get; set; }

    /// <summary>
    /// Gets or sets whether this supplement entry is active for the user.
    /// Defaults to true to support soft pause/deactivation behavior.
    /// </summary>
    public required bool IsActive { get; set; } = true;
}
