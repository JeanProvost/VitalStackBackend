using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Enums;
using Backend.Core.Services;
using Microsoft.Extensions.Configuration;

namespace Backend.Core.Tests.Supplements;

public class SupplementServiceTests
{
    [Fact]
    public async Task CreateStackEntryAsync_PreservesExplicitScheduleValues()
    {
        using var httpClient = new HttpClient();
        var service = new SupplementService(httpClient, new ConfigurationBuilder().Build());
        var request = new AddToStackRequest(
            null,
            "Custom supplement",
            "Capsule",
            "500 mg",
            "Custom brand",
            null,
            ScheduleTimeBlock.Evening,
            "Take with dinner");

        var entry = await service.CreateStackEntryAsync(
            request,
            "user-123",
            Array.Empty<SupplementProduct>().AsQueryable());

        Assert.Equal("user-123", entry.UserId);
        Assert.Equal("Custom supplement", entry.CustomName);
        Assert.Equal(ScheduleTimeBlock.Evening, entry.IntendedTime);
        Assert.Equal("Take with dinner", entry.ContextualInstruction);
        Assert.Equal("Evening", entry.Cusomization.TimeOfDayTarget);
        Assert.Equal(entry.CreatedAt, entry.UpdatedAt);
    }

    [Theory]
    [InlineData("withbreakfast", ScheduleTimeBlock.Morning)]
    [InlineData("withlunch", ScheduleTimeBlock.Afternoon)]
    [InlineData("withdinner", ScheduleTimeBlock.Evening)]
    [InlineData("beforebed", ScheduleTimeBlock.Night)]
    public async Task CreateStackEntryAsync_MapsLegacyTimeTargets(
        string timeOfDayTarget,
        ScheduleTimeBlock expected)
    {
        using var httpClient = new HttpClient();
        var service = new SupplementService(httpClient, new ConfigurationBuilder().Build());
        var request = new AddToStackRequest(
            null,
            "Custom supplement",
            null,
            null,
            null,
            timeOfDayTarget,
            null,
            null);

        var entry = await service.CreateStackEntryAsync(
            request,
            "user-123",
            Array.Empty<SupplementProduct>().AsQueryable());

        Assert.Equal(expected, entry.IntendedTime);
        Assert.Equal(timeOfDayTarget, entry.Cusomization.TimeOfDayTarget);
    }
}
