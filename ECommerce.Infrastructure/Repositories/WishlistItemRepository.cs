using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class WishlistItemRepository : IWishlistItemRepository
    {
        private readonly AppDbContext _context;

        public WishlistItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId) =>
            await _context.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

        public async Task<WishlistItem?> GetByIdAsync(int id) =>
            await _context.WishlistItems
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.Id == id);

        public async Task<WishlistItem?> GetByUserIdAndProductIdAsync(string userId, int productId) =>
            await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        public async Task AddAsync(WishlistItem wishlistItem)
        {
            await _context.WishlistItems.AddAsync(wishlistItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(WishlistItem wishlistItem)
        {
            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();
        }
    }
}
