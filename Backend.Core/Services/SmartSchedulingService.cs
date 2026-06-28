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
    public IReadOnlyList<ScheduledIntake> BuildSchedule(IEnumerable<Supplement> supplements)
    {
        ArgumentNullException.ThrowIfNull(supplements);

        var schedule = new List<ScheduledIntake>();

        foreach (var supplement in supplements)
        {
            var slot = DetermineSlot(supplement);
            schedule.Add(new ScheduledIntake(supplement, slot));
        }

        return schedule.AsReadOnly();
    }

    private static IntakeSlot DetermineSlot(Supplement supplement)
    {
        var name = supplement.Name.ToLowerInvariant();

        foreach (var keyword in PmKeywords)
        {
            if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return IntakeSlot.PM;
            }
        }

        foreach (var keyword in AmKeywords)
        {
            if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return IntakeSlot.AM;
            }
        }

        // Default to AM for unknown supplements (take with breakfast).
        return IntakeSlot.AM;
    }
}
