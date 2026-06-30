using Backend.Core.Entities.Supplements;
using Backend.Core.Enums;
using Backend.Core.Services;

namespace Backend.Core.Tests.SmartScheduling;

public sealed class SmartSchedulingServiceTests
{
    private readonly SmartSchedulingService _sut = new();

    // ── helpers ───────────────────────────────────────────────────────────────

    private static Supplement MakeSupplement(string name) => new()
    {
        Name = name,
        Form = "Capsule",
        DosageAmount = 1,
        DosageUnit = "capsule",
        TimeOfDay = "any"
    };

    // ── B-vitamin → AM ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Vitamin B12")]
    [InlineData("Vitamin B6")]
    [InlineData("B-Vitamin Complex")]
    [InlineData("Thiamine (B1)")]
    [InlineData("Riboflavin (B2)")]
    [InlineData("Niacin (B3)")]
    [InlineData("Pantothenic Acid (B5)")]
    [InlineData("Pyridoxine HCL")]
    [InlineData("Biotin (B7)")]
    [InlineData("Folate (B9)")]
    [InlineData("Methylcobalamin (B12)")]
    public void BVitamin_IsAssignedToAmSlot(string supplementName)
    {
        var schedule = _sut.BuildSchedule([MakeSupplement(supplementName)]);

        Assert.Single(schedule);
        Assert.Equal(IntakeSlot.AM, schedule[0].Slot);
    }

    // ── Magnesium → PM ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Magnesium Glycinate")]
    [InlineData("Magnesium Citrate")]
    [InlineData("Magnesium L-Threonate")]
    [InlineData("Magnesium")]
    public void Magnesium_IsAssignedToPmSlot(string supplementName)
    {
        var schedule = _sut.BuildSchedule([MakeSupplement(supplementName)]);

        Assert.Single(schedule);
        Assert.Equal(IntakeSlot.PM, schedule[0].Slot);
    }

    // ── Other PM-slot supplements ─────────────────────────────────────────────

    [Theory]
    [InlineData("Melatonin 3mg")]
    [InlineData("Ashwagandha Extract")]
    [InlineData("Glycine Powder")]
    [InlineData("L-Theanine")]
    [InlineData("Zinc Picolinate")]
    [InlineData("Calcium Carbonate")]
    public void PmSupplement_IsAssignedToPmSlot(string supplementName)
    {
        var schedule = _sut.BuildSchedule([MakeSupplement(supplementName)]);

        Assert.Single(schedule);
        Assert.Equal(IntakeSlot.PM, schedule[0].Slot);
    }

    // ── Mixed stack ───────────────────────────────────────────────────────────

    [Fact]
    public void MixedStack_AssignsBVitaminToAm_AndMagnesiumToPm()
    {
        var supplements = new[]
        {
            MakeSupplement("Vitamin B12"),
            MakeSupplement("Magnesium Glycinate"),
            MakeSupplement("Vitamin B6"),
            MakeSupplement("Magnesium Citrate")
        };

        var schedule = _sut.BuildSchedule(supplements);

        Assert.Equal(4, schedule.Count);
        Assert.All(
            schedule.Where(s => s.Supplement.Name.Contains("Vitamin B", StringComparison.OrdinalIgnoreCase)),
            entry => Assert.Equal(IntakeSlot.AM, entry.Slot));
        Assert.All(
            schedule.Where(s => s.Supplement.Name.Contains("Magnesium", StringComparison.OrdinalIgnoreCase)),
            entry => Assert.Equal(IntakeSlot.PM, entry.Slot));
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void EmptySupplementList_ReturnsEmptySchedule()
    {
        var schedule = _sut.BuildSchedule([]);

        Assert.Empty(schedule);
    }

    [Fact]
    public void UnknownSupplement_DefaultsToAmSlot()
    {
        var schedule = _sut.BuildSchedule([MakeSupplement("Turmeric Extract")]);

        Assert.Single(schedule);
        Assert.Equal(IntakeSlot.AM, schedule[0].Slot);
    }

    [Fact]
    public void NullSupplementList_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.BuildSchedule(null!));
    }

    // ── Case-insensitivity ────────────────────────────────────────────────────

    [Theory]
    [InlineData("MAGNESIUM OXIDE")]
    [InlineData("magnesium oxide")]
    [InlineData("Magnesium Oxide")]
    public void MagnesiumCasing_IsAlwaysAssignedToPmSlot(string supplementName)
    {
        var schedule = _sut.BuildSchedule([MakeSupplement(supplementName)]);

        Assert.Single(schedule);
        Assert.Equal(IntakeSlot.PM, schedule[0].Slot);
    }
}
