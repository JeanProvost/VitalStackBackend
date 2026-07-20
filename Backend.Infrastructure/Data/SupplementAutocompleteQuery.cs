using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data;

public static class SupplementAutocompleteQuery
{
    public const int MinimumQueryLength = 3;
    public const int ResultLimit = 10;

    public static IQueryable<SupplementAutocompleteSuggestionDto> Create(
        IQueryable<SupplementProduct> products,
        string trimmedQuery)
    {
        var escapedQuery = EscapeLikePattern(trimmedQuery);
        var prefixPattern = $"{escapedQuery}%";
        var substringPattern = $"%{escapedQuery}%";

        return products
            .AsNoTracking()
            .Where(product =>
                EF.Functions.ILike(product.ProductName, substringPattern, "\\") ||
                (product.BrandName != null &&
                    EF.Functions.ILike(product.BrandName, substringPattern, "\\")))
            .OrderByDescending(product =>
                EF.Functions.ILike(product.ProductName, substringPattern, "\\"))
            .ThenByDescending(product =>
                EF.Functions.ILike(product.ProductName, prefixPattern, "\\"))
            .ThenBy(product => product.ProductName)
            .ThenBy(product => product.Id)
            .Select(product => new SupplementAutocompleteSuggestionDto(
                product.Id,
                product.ProductName,
                product.BrandName))
            .Take(ResultLimit);
    }

    private static string EscapeLikePattern(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal);
}
