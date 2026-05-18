using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces
{
    public interface IWishlistItemRepository
    {
        Task<IEnumerable<WishlistItem>> GetByUserIdAsync(string userId);
        Task<WishlistItem?> GetByIdAsync(int id);
        Task<WishlistItem?> GetByUserIdAndProductIdAsync(string userId, int productId);
        Task AddAsync(WishlistItem wishlistItem);
        Task DeleteAsync(WishlistItem wishlistItem);
    }
}
