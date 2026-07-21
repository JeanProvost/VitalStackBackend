namespace Backend.Core.Entities.Supplements;

public class ProductIngredient
{
    public int ProductId { get; set; }
    public SupplementProduct Product { get; set; } = null!;

    public int IngredientId { get; set; }
    public SupplementIngredient Ingredient { get; set; } = null!;

    public required decimal DosageAmount { get; set; }
    public required string DosageUnit { get; set; }
}
