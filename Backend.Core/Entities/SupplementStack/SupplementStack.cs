using Backend.Core.Entities.Base;

namespace Backend.Core.Entities.SupplementStack;

/// <summary>
/// Represents a specific supplement entry in a user's personal stack.
/// This record ties a Cognito user identity to supplement dosage and timing metadata
/// used by interaction and chronobiology logic.
/// </summary>
public class SupplementStack : BaseEntity<Guid>
{
    /// <summary>
    /// Gets or sets the AWS Cognito subject (sub) identifier for tenant isolation.
    /// </summary>
    public required string UserId { get; set; }

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
