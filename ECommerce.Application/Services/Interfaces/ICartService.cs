using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Infrastructure.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDto>> GetCartByUserIdAsync(string userId);
        Task<CartItemDto> AddToCartAsync(string userId, int productId, int quantity);
        Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity);
        Task RemoveCartItemAsync(int cartItemId, string userId);
        /// <param name="cartKey">user id or guest key that owns the cart</param>
        /// <param name="orderUserId">null for a guest order</param>
        Task<int> CreateOrderFromCartAsync(string cartKey, string? orderUserId, string address, string? phone, string? customerName, string governorateKey);
        /// <summary>Moves a guest cart into the user's cart (same products are summed).</summary>
        Task MergeGuestCartAsync(string guestKey, string userId);
    }
}
