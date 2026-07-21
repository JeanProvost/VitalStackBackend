using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Core.Entities.Supplements;

public class SupplementIngredient
{
    public int Id { get; set; }
    public required string CanonicalName { get; set; }
    public required string Category { get; set; }

    [Column(TypeName = "jsonb")]
    public List<string> Aliases { get; set; } = new();

    public List<ProductIngredient> ProductLinks { get; set; } = new();
}
