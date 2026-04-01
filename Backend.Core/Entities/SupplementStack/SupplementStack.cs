using Backend.Core.Entities.Base;
using SupplementEntity = Backend.Core.Entities.Supplement.Supplement;

namespace Backend.Core.Entities.SupplementStack;

/// <summary>
/// Represents a user's supplement stack container.
/// A stack can contain multiple supplement entries.
/// </summary>
public class SupplementStack : BaseEntity<Guid>
{
    /// <summary>
    /// Gets or sets the AWS Cognito subject (sub) identifier for tenant isolation.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets whether this stack is active for the user.
    /// Defaults to true to support soft pause/deactivation behavior.
    /// </summary>
    public required bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets supplement entries that belong to this stack.
    /// </summary>
    public List<SupplementEntity> Supplements { get; set; } = [];
}
