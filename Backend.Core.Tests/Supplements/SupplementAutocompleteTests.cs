using Backend.API.Endpoints;
using Backend.Core.Entities.Supplements.DTOs;
using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Tests.Supplements;

public class SupplementAutocompleteTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AutocompleteAsync_BlankQuery_ReturnsBadRequest(string? query)
    {
        await using var db = CreateDbContext();

        var result = await SupplementEndpoints.AutocompleteAsync(query, db, CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData(" ab ")]
    public async Task AutocompleteAsync_TrimmedQueryShorterThanThreeCharacters_ReturnsEmptyArray(string query)
    {
        await using var db = CreateDbContext();

        var result = await SupplementEndpoints.AutocompleteAsync(query, db, CancellationToken.None);

        var okResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        Assert.Empty(Assert.IsType<SupplementAutocompleteSuggestionDto[]>(okResult.Value));
    }

    [Fact]
    public void Create_RanksProductMatchesAndLimitsProjectedShape()
    {
        using var db = CreateDbContext();

        var sql = SupplementAutocompleteQuery.Create(db.SupplementProducts, "mag").ToQueryString();

        Assert.Contains("LIMIT @", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY", sql, StringComparison.Ordinal);
        Assert.Contains("\"ProductName\" ILIKE", sql, StringComparison.Ordinal);
        Assert.Contains("DESC", sql, StringComparison.Ordinal);
        Assert.Contains("\"ProductName\", s.\"Id\"", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("ActiveIngredients", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("Metadata", sql, StringComparison.Ordinal);
        Assert.Equal(10, SupplementAutocompleteQuery.ResultLimit);
    }

    [Fact]
    public void SuggestionDto_HasOnlyRequiredFields()
    {
        var properties = typeof(SupplementAutocompleteSuggestionDto)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(["BrandName", "Id", "ProductName"], properties);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=autocomplete_query_tests;Username=test;Password=test")
            .Options;

        return new ApplicationDbContext(options);
    }
}
