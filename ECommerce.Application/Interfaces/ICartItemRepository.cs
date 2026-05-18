using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces
{
    public interface ICartItemRepository
    {
        Task<IEnumerable<CartItem>> GetByUserIdAsync(string userId);
        Task<CartItem?> GetByIdAsync(int id);
        Task<CartItem?> GetByUserIdAndProductIdAsync(string userId, int productId);
        Task AddAsync(CartItem cartItem);
        Task UpdateAsync(CartItem cartItem);
        Task DeleteAsync(CartItem cartItem);
        Task DeleteByUserIdAsync(string userId);
        /// <summary>Returns cart items created before the given cutoff (e.g. for expiry cleanup).</summary>
        Task<IReadOnlyList<CartItem>> GetOlderThanAsync(DateTime cutoffUtc);
    }
}
