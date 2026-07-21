using Backend.Core.Entities.Supplements;
using Backend.Core.Enums;
using Backend.Core.Interfaces.IServices;

namespace Backend.Core.Services;

/// <summary>
/// Implements chronobiology-based scheduling rules (F-002: Smart Scheduling).
/// Rules are deterministic business logic and must remain in the .NET monolith.
/// </summary>
public class SchedulingService : ISchedulingService
{
    // Supplements with stimulatory or energy-supporting properties belong in the AM slot.
    private static readonly string[] AmKeywords =
    [
        "vitamin b", "b-vitamin", "b complex",
        "b1", "thiamine",
        "b2", "riboflavin",
        "b3", "niacin",
        "b6", "pyridoxine",
        "b12", "cobalamin",
        "folate", "folic acid",
        "biotin",
        "vitamin c",
        "iron",
        "coq10", "coenzyme q10"
    ];

    // Supplements with calming, sleep-supporting, or recovery properties belong in the PM slot.
    private static readonly string[] PmKeywords =
    [
        "magnesium",
        "melatonin",
        "ashwagandha",
        "l-theanine",
        "glycine",
        "zinc"
    ];

    public ScheduleTimeBlock RecommendTimeBlock(SupplementProduct supplementProduct)
    {
        ArgumentNullException.ThrowIfNull(supplementProduct);

        if (ContainsAnySearchTerm(supplementProduct, PmKeywords))
            return ScheduleTimeBlock.Evening;

        if (ContainsAnySearchTerm(supplementProduct, AmKeywords))
            return ScheduleTimeBlock.Morning;

        // Default: morning with food is the safest universal fallback.
        return ScheduleTimeBlock.Morning;
    }

    private static bool ContainsAnySearchTerm(SupplementProduct supplementProduct, IReadOnlyCollection<string> keywords)
    {
        return GetSearchTerms(supplementProduct)
            .Any(term => keywords.Any(keyword => term.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
    }

    private static IEnumerable<string> GetSearchTerms(SupplementProduct supplementProduct)
    {
        yield return supplementProduct.ProductName;

        foreach (var productIngredient in supplementProduct.ActiveIngredients)
        {
            yield return productIngredient.Ingredient.CanonicalName;

            foreach (var alias in productIngredient.Ingredient.Aliases)
            {
                yield return alias;
            }
        }
    }
}
