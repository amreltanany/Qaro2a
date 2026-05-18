using ECommerce.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.API.BackgroundServices;

/// <summary>
/// Runs once per day and removes cart items older than 2 days, restoring product stock.
/// </summary>
public class CartExpiryHostedService : BackgroundService
{
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
        using var scope = _scopeFactory.CreateScope();
        var cartRepo = scope.ServiceProvider.GetRequiredService<ICartItemRepository>();
        var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var oldItems = await cartRepo.GetOlderThanAsync(cutoff);
        if (oldItems.Count == 0)
        {
            _logger.LogDebug("No cart items older than {Cutoff} to remove.", cutoff);
            return;
        }

        _logger.LogInformation("Removing {Count} cart item(s) older than {Cutoff}.", oldItems.Count, cutoff);

        foreach (var item in oldItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var product = await productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.AddStock(item.Quantity);
                    await productRepo.UpdateAsync(product);
                }
                await cartRepo.DeleteAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove cart item Id={ItemId}, ProductId={ProductId}.", item.Id, item.ProductId);
            }
        }
    }
}
