using AutoMapper;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;

namespace ECommerce.Application.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CartService(
            ICartItemRepository cartItemRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _cartItemRepository = cartItemRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItemDto>> GetCartByUserIdAsync(string userId)
        {
            var items = await _cartItemRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<CartItemDto>>(items);
        }

        public async Task<CartItemDto> AddToCartAsync(string userId, int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) throw new KeyNotFoundException($"Product {productId} not found");
            if (product.Stock < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

            var existing = await _cartItemRepository.GetByUserIdAndProductIdAsync(userId, productId);
            if (existing != null)
            {
                existing.SetQuantity(existing.Quantity + quantity);
                await _cartItemRepository.UpdateAsync(existing);
                product.ReduceStock(quantity);
                await _productRepository.UpdateAsync(product);
                var updated = await _cartItemRepository.GetByIdAsync(existing.Id);
                return _mapper.Map<CartItemDto>(updated!);
            }

            var cartItem = new CartItem(userId, productId, product.Price, quantity);
            await _cartItemRepository.AddAsync(cartItem);
            product.ReduceStock(quantity);
            await _productRepository.UpdateAsync(product);
            var added = await _cartItemRepository.GetByIdAsync(cartItem.Id);
            return _mapper.Map<CartItemDto>(added!);
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, string userId, int quantity)
        {
            var item = await _cartItemRepository.GetByIdAsync(cartItemId);
            if (item == null) throw new Exception("Cart item not found");
            if (item.UserId != userId) throw new UnauthorizedAccessException("Cart item does not belong to user");

            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null) throw new Exception("Product not found");

            int oldQty = item.Quantity;
            int delta = quantity - oldQty;
            if (delta > 0)
            {
                if (product.Stock < delta)
                    throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");
                product.ReduceStock(delta);
                await _productRepository.UpdateAsync(product);
            }
            else if (delta < 0)
            {
                product.AddStock(-delta);
                await _productRepository.UpdateAsync(product);
            }

            item.SetQuantity(quantity);
            await _cartItemRepository.UpdateAsync(item);
        }

        public async Task RemoveCartItemAsync(int cartItemId, string userId)
        {
            var item = await _cartItemRepository.GetByIdAsync(cartItemId);
            if (item == null) throw new Exception("Cart item not found");
            if (item.UserId != userId) throw new UnauthorizedAccessException("Cart item does not belong to user");

            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.AddStock(item.Quantity);
                await _productRepository.UpdateAsync(product);
            }
            await _cartItemRepository.DeleteAsync(item);
        }

        public async Task CreateOrderFromCartAsync(string userId, string address, string? phone)
        {
            var cartItems = (await _cartItemRepository.GetByUserIdAsync(userId)).ToList();
            if (cartItems.Count == 0) throw new InvalidOperationException("Cart is empty");

            var orderItems = new List<OrderItem>();
            foreach (var item in cartItems)
            {
                orderItems.Add(new OrderItem(item.ProductId, item.Price, item.Quantity));
            }
            var order = new Order(userId, orderItems);
            order.SetShippingAddress(address);
            order.SetShippingPhone(phone);

            await _orderRepository.AddAsync(order);
            await _cartItemRepository.DeleteByUserIdAsync(userId);
        }
    }
}
