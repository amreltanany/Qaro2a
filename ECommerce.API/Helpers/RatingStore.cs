using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Helpers;

/// <summary>Star ratings: averages for the cards, a visitor's own rating, and the upsert behind the stars.</summary>
public class RatingStore
{
    private readonly AppDbContext _db;
    public RatingStore(AppDbContext db) => _db = db;

    public async Task<Dictionary<int, (double Avg, int Count)>> SummariesAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0) return new();
        var rows = await _db.ProductRatings.AsNoTracking()
            .Where(r => ids.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { g.Key, Avg = g.Average(r => (double)r.Stars), Count = g.Count() })
            .ToListAsync();
        return rows.ToDictionary(r => r.Key, r => (Math.Round(r.Avg, 1), r.Count));
    }

    public async Task<Dictionary<int, int>> MineAsync(string? ownerKey, IEnumerable<int> productIds)
    {
        if (string.IsNullOrEmpty(ownerKey)) return new();
        var ids = productIds.Distinct().ToList();
        return await _db.ProductRatings.AsNoTracking()
            .Where(r => r.OwnerKey == ownerKey && ids.Contains(r.ProductId))
            .ToDictionaryAsync(r => r.ProductId, r => r.Stars);
    }

    /// <summary>Saves (or changes) this visitor's rating and returns the book's new average and count; null when the book does not exist.</summary>
    public async Task<(double Avg, int Count)?> RateAsync(int productId, string ownerKey, int stars)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == productId)) return null;
        var mine = await _db.ProductRatings.FirstOrDefaultAsync(r => r.ProductId == productId && r.OwnerKey == ownerKey);
        if (mine == null) _db.ProductRatings.Add(new ProductRating(productId, ownerKey, stars));
        else mine.SetStars(stars);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { /* two taps at once hit the unique index: the other one already saved this visitor's rating */ }
        var s = await SummariesAsync(new[] { productId });
        return s.TryGetValue(productId, out var v) ? v : (0, 0);
    }
}
