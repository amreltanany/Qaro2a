using AutoMapper;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;

namespace ECommerce.Application.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistItemRepository _wishlistItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public WishlistService(
            IWishlistItemRepository wishlistItemRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _wishlistItemRepository = wishlistItemRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WishlistItemDto>> GetWishlistByUserIdAsync(string userId)
        {
            var items = await _wishlistItemRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<WishlistItemDto>>(items);
        }

        public async Task<WishlistItemDto> AddToWishlistAsync(string userId, int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) throw new Exception($"Product {productId} not found");

            var existing = await _wishlistItemRepository.GetByUserIdAndProductIdAsync(userId, productId);
            if (existing != null)
            {
                var existingWithProduct = await _wishlistItemRepository.GetByIdAsync(existing.Id);
                return _mapper.Map<WishlistItemDto>(existingWithProduct!);
            }

            var wishlistItem = new WishlistItem(userId, productId);
            await _wishlistItemRepository.AddAsync(wishlistItem);
            var added = await _wishlistItemRepository.GetByIdAsync(wishlistItem.Id);
            return _mapper.Map<WishlistItemDto>(added!);
        }

        public async Task RemoveWishlistItemAsync(int wishlistItemId, string userId)
        {
            var item = await _wishlistItemRepository.GetByIdAsync(wishlistItemId);
            if (item == null) throw new Exception("Wishlist item not found");
            if (item.UserId != userId) throw new UnauthorizedAccessException("Wishlist item does not belong to user");
            await _wishlistItemRepository.DeleteAsync(item);
        }
    }
}
