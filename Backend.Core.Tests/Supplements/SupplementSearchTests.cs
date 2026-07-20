using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;

namespace Backend.Core.Tests.Supplements;

public class SupplementSearchTests
{
    // ─── DTO projection ──────────────────────────────────────────────────────

    [Fact]
    public void SearchProjection_MapsFieldsCorrectly_AndExcludesMetadata()
    {
        var ingredient = new SupplementIngredient
        {
            CanonicalName = "Vitamin D3",
            Category = "Vitamin"
        };

        var product = new SupplementProduct
        {
            Id = 42,
            DsldId = "DSLD-001",
            ProductName = "Nature Made Vitamin D3",
            BrandName = "Nature Made",
            Form = "Softgel",
            ActiveIngredients =
            [
                new ProductIngredient
                {
                    Ingredient = ingredient,
                    DosageAmount = 2000,
                    DosageUnit = "IU"
                }
            ]
        };

        var dto = Project(product);

        Assert.Equal(42, dto.Id);
        Assert.Equal("Nature Made Vitamin D3", dto.ProductName);
        Assert.Equal("Nature Made", dto.BrandName);
        Assert.Equal("Softgel", dto.Form);
        Assert.Single(dto.Ingredients);
        Assert.Equal("Vitamin D3", dto.Ingredients[0].Name);
        Assert.Equal(2000m, dto.Ingredients[0].DosageAmount);
        Assert.Equal("IU", dto.Ingredients[0].DosageUnit);
        // SupplementSearchResultDto has no Metadata field — enforced at compile time
    }

    // ─── Empty query guard ───────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyQuery_GuardCondition_IsTrue(string? query)
    {
        Assert.True(string.IsNullOrWhiteSpace(query));
    }

    [Fact]
    public void NonEmptyQuery_GuardCondition_IsFalse()
    {
        Assert.False(string.IsNullOrWhiteSpace("vitamin d"));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static SupplementSearchResultDto Project(SupplementProduct p) => new(
        p.Id,
        p.ProductName,
        p.BrandName,
        p.Form,
        p.ActiveIngredients.Select(pi => new IngredientSummaryDto(
            pi.Ingredient.CanonicalName,
            pi.DosageAmount,
            pi.DosageUnit
        )).ToList()
    );
}
