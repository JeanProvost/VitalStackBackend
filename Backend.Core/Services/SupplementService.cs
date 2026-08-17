using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Services;

public class SupplementService
{
    private const int SearchResultLimit = 25;
    public const int AutocompleteMinimumQueryLength = 3;
    public const int AutocompleteResultLimit = 10;

    public async Task<IReadOnlyList<SupplementSearchResultDto>> SearchAsync(
        IQueryable<SupplementProduct> supplementProducts,
        string query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(supplementProducts);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var searchPattern = $"%{query.Trim()}%";

        // Product-name matches have absolute priority. Fetching these first avoids calculating
        // ingredient percentages for thousands of products that cannot reach the response.
        var rankedProductIds = await supplementProducts
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.ProductName, searchPattern))
            .OrderBy(p => p.ProductName)
            .ThenBy(p => p.Id)
            .Select(p => p.Id)
            .Take(SearchResultLimit)
            .ToListAsync(cancellationToken);

        var remaining = SearchResultLimit - rankedProductIds.Count;
        if (remaining > 0)
        {
            var ingredientProductIds = await supplementProducts
                .AsNoTracking()
                .Where(p => !rankedProductIds.Contains(p.Id) &&
                    p.ActiveIngredients.Any(pi =>
                        EF.Functions.ILike(pi.Ingredient.CanonicalName, searchPattern)))
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    // Convert mass units to mg so a 1 g ingredient is not ranked below 500 mg.
                    MatchingIngredientMass = p.ActiveIngredients
                        .Where(pi => EF.Functions.ILike(pi.Ingredient.CanonicalName, searchPattern))
                        .Sum(pi => pi.DosageUnit.ToLower() == "g" ? pi.DosageAmount * 1000m :
                            pi.DosageUnit.ToLower() == "mg" ? pi.DosageAmount :
                            pi.DosageUnit.ToLower() == "mcg" ? pi.DosageAmount / 1000m : 0m),
                    TotalIngredientMass = p.ActiveIngredients
                        .Sum(pi => pi.DosageUnit.ToLower() == "g" ? pi.DosageAmount * 1000m :
                            pi.DosageUnit.ToLower() == "mg" ? pi.DosageAmount :
                            pi.DosageUnit.ToLower() == "mcg" ? pi.DosageAmount / 1000m : 0m)
                })
                .OrderByDescending(x => x.TotalIngredientMass == 0m
                    ? 0m
                    : x.MatchingIngredientMass / x.TotalIngredientMass)
                .ThenBy(x => x.ProductName)
                .ThenBy(x => x.Id)
                .Select(x => x.Id)
                .Take(remaining)
                .ToListAsync(cancellationToken);

            rankedProductIds.AddRange(ingredientProductIds);
            remaining -= ingredientProductIds.Count;
        }

        if (remaining > 0)
        {
            var brandProductIds = await supplementProducts
                .AsNoTracking()
                .Where(p => !rankedProductIds.Contains(p.Id) &&
                    p.BrandName != null &&
                    EF.Functions.ILike(p.BrandName, searchPattern))
                .OrderBy(p => p.ProductName)
                .ThenBy(p => p.Id)
                .Select(p => p.Id)
                .Take(remaining)
                .ToListAsync(cancellationToken);

            rankedProductIds.AddRange(brandProductIds);
        }

        var productResults = await supplementProducts
            .AsNoTracking()
            .Where(p => rankedProductIds.Contains(p.Id))
            .Select(p => new SupplementSearchResultDto(
                p.Id,
                p.ProductName,
                p.BrandName,
                p.Form,
                p.ActiveIngredients.Select(pi => new IngredientSummaryDto(
                    pi.Ingredient.CanonicalName,
                    pi.DosageAmount,
                    pi.DosageUnit
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        var resultsByProductId = productResults.ToDictionary(result => result.Id);
        return rankedProductIds.Select(id => resultsByProductId[id]).ToList();
    }

    public async Task<IReadOnlyList<SupplementAutocompleteSuggestionDto>> AutocompleteAsync(
        IQueryable<SupplementProduct> supplementProducts,
        string query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(supplementProducts);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length < AutocompleteMinimumQueryLength)
        {
            return Array.Empty<SupplementAutocompleteSuggestionDto>();
        }

        return await CreateAutocompleteQuery(supplementProducts, trimmedQuery)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<SupplementAutocompleteSuggestionDto> CreateAutocompleteQuery(
        IQueryable<SupplementProduct> supplementProducts,
        string trimmedQuery)
    {
        ArgumentNullException.ThrowIfNull(supplementProducts);
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedQuery);

        var escapedQuery = EscapeLikePattern(trimmedQuery);
        var prefixPattern = $"{escapedQuery}%";
        var substringPattern = $"%{escapedQuery}%";

        return supplementProducts
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
            .Take(AutocompleteResultLimit);
    }

    private static string EscapeLikePattern(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal);
}
