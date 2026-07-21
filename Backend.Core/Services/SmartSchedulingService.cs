using Backend.Core.Entities.Supplements;
using Backend.Core.Enums;
using Backend.Core.Interfaces.IServices;

namespace Backend.Core.Services;

/// <summary>
/// Assigns supplements to AM or PM intake slots based on chronobiology rules.
/// This is deterministic business logic and belongs entirely in the .NET monolith.
/// </summary>
public sealed class SmartSchedulingService : ISmartSchedulingService
{
    // Supplement name fragments that map to the AM slot (energising / stimulatory).
    private static readonly string[] AmKeywords =
    [
        "vitamin b", "b-vitamin",
        "b1", "thiamine",
        "b2", "riboflavin",
        "b3", "niacin",
        "b5", "pantothenic",
        "b6", "pyridoxine",
        "b7", "biotin",
        "b9", "folate", "folic acid",
        "b12", "cobalamin",
        "vitamin c",
        "iron",
        "coq10", "coenzyme q10"
    ];

    // Supplement name fragments that map to the PM slot (relaxing / recovery).
    private static readonly string[] PmKeywords =
    [
        "magnesium",
        "melatonin",
        "glycine",
        "l-theanine",
        "ashwagandha",
        "zinc",
        "calcium"
    ];

    /// <inheritdoc />
    public IReadOnlyList<ScheduledIntake> BuildSchedule(IEnumerable<SupplementProduct> supplementProducts)
    {
        ArgumentNullException.ThrowIfNull(supplementProducts);

        var schedule = new List<ScheduledIntake>();

        foreach (var supplementProduct in supplementProducts)
        {
            var slot = DetermineSlot(supplementProduct);
            schedule.Add(new ScheduledIntake(supplementProduct, slot));
        }

        return schedule.AsReadOnly();
    }

    private static IntakeSlot DetermineSlot(SupplementProduct supplementProduct)
    {
        if (ContainsAnySearchTerm(supplementProduct, PmKeywords))
            return IntakeSlot.PM;

        if (ContainsAnySearchTerm(supplementProduct, AmKeywords))
            return IntakeSlot.AM;

        // Default to AM for unknown supplements (take with breakfast).
        return IntakeSlot.AM;
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
