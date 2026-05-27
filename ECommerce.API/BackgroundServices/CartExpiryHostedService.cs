using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.API.BackgroundServices;

/// <summary>
/// Runs once per day and removes cart items older than 2 days, restoring product stock.
/// </summary>
public class CartExpiryHostedService : BackgroundService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CartExpiryAfter = TimeSpan.FromDays(2);
    private static readonly TimeSpan RunInterval = TimeSpan.FromDays(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CartExpiryHostedService> _logger;

    public CartExpiryHostedService(IServiceScopeFactory scopeFactory, ILogger<CartExpiryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cart expiry hosted service started. Will clear carts older than {Days} days once per day.",
            CartExpiryAfter.TotalDays);

        _logger.LogInformation("Delaying first cart cleanup run for {DelayMinutes} minutes to avoid startup contention.",
            InitialDelay.TotalMinutes);
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldCartItemsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cart expiry cleanup.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private async Task CleanupOldCartItemsAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - CartExpiryAfter;
        var startedAt = DateTime.UtcNow;
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var oldItems = await db.CartItems
            .Where(c => c.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);
        if (oldItems.Count == 0)
        {
            _logger.LogDebug("No cart items older than {Cutoff} to remove.", cutoff);
            return;
        }

        _logger.LogInformation("Removing {Count} cart item(s) older than {Cutoff}.", oldItems.Count, cutoff);

        var qtyByProduct = oldItems
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var productIds = qtyByProduct.Keys.ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (qtyByProduct.TryGetValue(product.Id, out var qtyToRestore) && qtyToRestore > 0)
                product.AddStock(qtyToRestore);
        }

        db.CartItems.RemoveRange(oldItems);
        await db.SaveChangesAsync(cancellationToken);

        var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
        _logger.LogInformation("Cart cleanup completed in {ElapsedMs} ms.", elapsedMs);
    }
}
