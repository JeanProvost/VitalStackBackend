using Backend.Core.Entities.Supplements;
using Backend.Core.Entities.Supplements.DTOs;
using Backend.Core.Entities.UserStackEntries;
using Backend.Core.Enums;
using Backend.Core.Services;
using Microsoft.Extensions.Configuration;

namespace Backend.Core.Tests.Supplements;

public class StackServiceTests
{
    [Fact]
    public async Task CreateEntryAsync_CreatesOneCatalogProductEntry()
    {
        using var httpClient = new HttpClient();
        var service = new StackService(httpClient, new ConfigurationBuilder().Build());
        var product = CreateProduct(42);
        var request = new AddToStackRequest(
            product.DsldId,
            1.5m,
            ScheduleTimeBlock.Evening,
            "Take with dinner");

        var (entry, isDuplicate) = await service.CreateEntryAsync(
            request,
            "user-123",
            new[] { product }.AsQueryable(),
            new[]
            {
                new UserStackEntry
                {
                    UserId = "other-user",
                    SupplementProductId = product.Id
                }
            }.AsQueryable());

        Assert.NotNull(entry);
        Assert.False(isDuplicate);
        Assert.Equal("user-123", entry.UserId);
        Assert.Equal(product.Id, entry.SupplementProductId);
        Assert.Null(entry.CustomName);
        Assert.Equal(1.5m, entry.ServingMultiplier);
        Assert.Equal(ScheduleTimeBlock.Evening, entry.IntendedTime);
        Assert.Equal("Take with dinner", entry.ContextualInstruction);
        Assert.Equal(entry.CreatedAt, entry.UpdatedAt);
    }

    [Fact]
    public async Task CreateEntryAsync_MissingCatalogProduct_ReturnsNull()
    {
        using var httpClient = new HttpClient();
        var service = new StackService(httpClient, new ConfigurationBuilder().Build());
        var request = new AddToStackRequest("10000");

        var (entry, isDuplicate) = await service.CreateEntryAsync(
            request,
            "user-123",
            new[] { CreateProduct(42) }.AsQueryable(),
            Array.Empty<UserStackEntry>().AsQueryable());

        Assert.Null(entry);
        Assert.False(isDuplicate);
    }

    [Fact]
    public async Task CreateEntryAsync_SameProductForSameUser_ReturnsDuplicate()
    {
        using var httpClient = new HttpClient();
        var service = new StackService(httpClient, new ConfigurationBuilder().Build());
        var product = CreateProduct(42);
        var request = new AddToStackRequest(product.DsldId);

        var (entry, isDuplicate) = await service.CreateEntryAsync(
            request,
            "user-123",
            new[] { product }.AsQueryable(),
            new[]
            {
                new UserStackEntry
                {
                    UserId = "user-123",
                    SupplementProductId = product.Id
                }
            }.AsQueryable());

        Assert.Null(entry);
        Assert.True(isDuplicate);
    }

    [Fact]
    public void AddToStackRequest_ContainsOnlySingleProductAndStackSettings()
    {
        var properties = typeof(AddToStackRequest)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            ["ContextualInstruction", "IntendedTime", "ServingMultiplier", "SupplementProductId"],
            properties);
    }

    private static SupplementProduct CreateProduct(int id) => new()
    {
        Id = id,
        DsldId = $"DSLD-{id}",
        ProductName = "Magnesium Glycinate",
        BrandName = "Example Brand",
        Form = "Capsule",
        ActiveIngredients =
        [
            new ProductIngredient
            {
                Ingredient = new SupplementIngredient
                {
                    CanonicalName = "Magnesium",
                    Category = "Mineral"
                },
                DosageAmount = 200m,
                DosageUnit = "mg"
            }
        ]
    };
}
