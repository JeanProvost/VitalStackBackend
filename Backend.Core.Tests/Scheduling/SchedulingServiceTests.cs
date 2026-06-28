using Backend.Core.Entities.Supplements;
using Backend.Core.Enums;
using Backend.Core.Services;

namespace Backend.Core.Tests.Scheduling;

/// <summary>
/// Unit tests for F-002: Smart Scheduling.
/// Validates chronobiology-based rules without any database or external-service dependencies.
/// </summary>
public class SchedulingServiceTests
{
    private readonly SchedulingService _sut = new();

    // ─── AM rules ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Vitamin B Complex")]
    [InlineData("Vitamin B12")]
    [InlineData("Vitamin B6")]
    [InlineData("B1 (Thiamine)")]
    [InlineData("B2 Riboflavin")]
    [InlineData("B3 Niacin")]
    [InlineData("Methylcobalamin (B12)")]
    [InlineData("Folate")]
    [InlineData("Folic Acid")]
    [InlineData("Biotin")]
    public void RecommendTimeBlock_BVitamins_ReturnsMorning(string supplementName)
    {
        var supplement = BuildSupplement(supplementName);

        var result = _sut.RecommendTimeBlock(supplement);

        Assert.Equal(ScheduleTimeBlock.Morning, result);
    }

    // ─── PM rules ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Magnesium Glycinate")]
    [InlineData("Magnesium Citrate")]
    [InlineData("Magnesium")]
    [InlineData("Melatonin 3mg")]
    [InlineData("Ashwagandha Extract")]
    [InlineData("L-Theanine")]
    [InlineData("Glycine")]
    [InlineData("Zinc Picolinate")]
    public void RecommendTimeBlock_PmSupplements_ReturnsEvening(string supplementName)
    {
        var supplement = BuildSupplement(supplementName);

        var result = _sut.RecommendTimeBlock(supplement);

        Assert.Equal(ScheduleTimeBlock.Evening, result);
    }

    // ─── Core F-002 chronobiology contract ───────────────────────────────────

    [Fact]
    public void RecommendTimeBlock_BVitamin_IsAssignedToAmSlot()
    {
        var bVitamin = BuildSupplement("Vitamin B Complex");

        var result = _sut.RecommendTimeBlock(bVitamin);

        Assert.Equal(ScheduleTimeBlock.Morning, result);
    }

    [Fact]
    public void RecommendTimeBlock_Magnesium_IsAssignedToPmSlot()
    {
        var magnesium = BuildSupplement("Magnesium Glycinate");

        var result = _sut.RecommendTimeBlock(magnesium);

        Assert.Equal(ScheduleTimeBlock.Evening, result);
    }

    // ─── Null guard ───────────────────────────────────────────────────────────

    [Fact]
    public void RecommendTimeBlock_NullSupplement_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.RecommendTimeBlock(null!));
    }

    // ─── Default fallback ────────────────────────────────────────────────────

    [Fact]
    public void RecommendTimeBlock_UnknownSupplement_DefaultsToMorning()
    {
        var unknown = BuildSupplement("SomeUnknownHerb XYZ");

        var result = _sut.RecommendTimeBlock(unknown);

        Assert.Equal(ScheduleTimeBlock.Morning, result);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Supplement BuildSupplement(string name) => new()
    {
        Name = name,
        Form = "Capsule",
        DosageAmount = 1,
        DosageUnit = "capsule",
        TimeOfDay = "AM"
    };
}
