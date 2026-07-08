using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Backend.Core.Entities.Supplements;

public class SupplementProduct
{
    public int Id { get; set; }
    public required string DsldId { get; set; }
    public required string ProductName { get; set; }
    public string? BrandName { get; set; }
    public required string Form { get; set; }

    [Column(TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    public List<ProductIngredient> ActiveIngredients { get; set; } = new();
}
