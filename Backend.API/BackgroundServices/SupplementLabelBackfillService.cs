using Backend.Core.Services;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.BackgroundServices;

public sealed class SupplementLabelBackfillService(
    IServiceScopeFactory scopeFactory,
    ILogger<SupplementLabelBackfillService> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan BatchDelay = TimeSpan.FromMilliseconds(500);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var lastProductId = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var supplementService = scope.ServiceProvider.GetRequiredService<SupplementService>();

                var products = await supplementService
                    .CreateLabelAssetBackfillQuery(
                        db.SupplementProducts,
                        lastProductId,
                        BatchSize)
                    .ToListAsync(stoppingToken);

                await supplementService.PopulateLabelAssetsAsync(products, stoppingToken);

                await db.SaveChangesAsync(stoppingToken);

                if (products.Count < BatchSize)
                {
                    return;
                }

                lastProductId = products[^1].Id;
                await Task.Delay(BatchDelay, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Supplement label backfill stopped unexpectedly");
        }
    }
}
