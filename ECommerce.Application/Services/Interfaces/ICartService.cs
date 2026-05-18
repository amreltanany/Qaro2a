using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Infrastructure.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDto>> GetCartByUserIdAsync(string userId);
        Task<CartItemDto> AddToCartAsync(string userId, int productId, int quantity);
        Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity);
        Task RemoveCartItemAsync(int cartItemId, string userId);
        Task CreateOrderFromCartAsync(string userId, string address, string? phone);
    }
}
