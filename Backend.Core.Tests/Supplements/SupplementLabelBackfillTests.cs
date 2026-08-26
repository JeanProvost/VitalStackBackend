using Backend.Core.Entities.Supplements;
using Backend.Core.Services;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace Backend.Core.Tests.Supplements;

public class SupplementLabelBackfillTests
{
    [Fact]
    public void CreateLabelAssetBackfillQuery_SelectsOnlyUncheckedProductsAfterCursor()
    {
        using var db = CreateDbContext();
        using var httpClient = new HttpClient();
        var service = new SupplementService(httpClient);

        var sql = service
            .CreateLabelAssetBackfillQuery(db.SupplementProducts, 100, 50)
            .ToQueryString();

        Assert.Contains("LabelAssetsFetchedAtUtc", sql, StringComparison.Ordinal);
        Assert.Contains("IS NULL", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY", sql, StringComparison.Ordinal);
        Assert.Contains("LIMIT @", sql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PopulateLabelAssetsAsync_SavesReturnedUrlsOnProduct()
    {
        Uri? requestedUri = null;
        using var handler = new StubHttpMessageHandler(request =>
        {
            requestedUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    thumbnail = "https://example.test/thumbnail.jpg",
                    pdf = "https://example.test/label.pdf"
                })
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = new SupplementService(httpClient);
        var product = CreateProduct();

        await service.PopulateLabelAssetsAsync([product]);

        Assert.Equal(
            "https://api.ods.od.nih.gov/dsld/v9/label/12345",
            requestedUri?.AbsoluteUri);
        Assert.Equal(
            "https://api.ods.od.nih.gov/dsld/s3/pdf/thumbnails/12345.jpg",
            product.ThumbnailUrl);
        Assert.Equal(
            "https://api.ods.od.nih.gov/dsld/s3/pdf/12345.pdf",
            product.LabelPdfUrl);
        Assert.NotNull(product.LabelAssetsFetchedAtUtc);
    }

    [Fact]
    public async Task PopulateLabelAssetsAsync_EmptyAssetsStillMarksProductChecked()
    {
        using var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { thumbnail = "", pdf = " " })
            });
        using var httpClient = new HttpClient(handler);
        var service = new SupplementService(httpClient);
        var product = CreateProduct();

        await service.PopulateLabelAssetsAsync([product]);

        Assert.Null(product.ThumbnailUrl);
        Assert.Null(product.LabelPdfUrl);
        Assert.NotNull(product.LabelAssetsFetchedAtUtc);
    }

    [Fact]
    public async Task PopulateLabelAssetsAsync_UnsuccessfulResponseLeavesProductUnchecked()
    {
        using var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var httpClient = new HttpClient(handler);
        var service = new SupplementService(httpClient);
        var product = CreateProduct();

        await service.PopulateLabelAssetsAsync([product]);

        Assert.Null(product.ThumbnailUrl);
        Assert.Null(product.LabelPdfUrl);
        Assert.Null(product.LabelAssetsFetchedAtUtc);
    }

    private static SupplementProduct CreateProduct() => new()
    {
        Id = 101,
        DsldId = "12345",
        ProductName = "Test Supplement",
        Form = "Capsule"
    };

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=label_backfill_query_tests;Username=test;Password=test")
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> handle) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(handle(request));
    }
}
