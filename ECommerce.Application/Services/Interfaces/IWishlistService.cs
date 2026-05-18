using ECommerce.Application.DTOs.Wishlist;

namespace ECommerce.Infrastructure.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItemDto>> GetWishlistByUserIdAsync(string userId);
        Task<WishlistItemDto> AddToWishlistAsync(string userId, int productId);
        Task RemoveWishlistItemAsync(int wishlistItemId, string userId);
    }
}
