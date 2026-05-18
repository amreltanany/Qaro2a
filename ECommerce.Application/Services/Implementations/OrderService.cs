using AutoMapper;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo, IMapper mapper)
    {
        _orderRepository = orderRepo;
        _productRepository = productRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(string userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) throw new Exception("Order not found");

        return _mapper.Map<OrderResponseDto>(order);
    }

    public async Task<OrderResponseDto> AddOrderAsync(OrderCreateDto dto)
    {
        // 1. Create the Entity (null UserId for guest checkout)
        var userId = string.IsNullOrEmpty(dto.UserId) || dto.UserId == "guest" ? null : dto.UserId;
        var order = new Order(userId, new List<OrderItem>());

        // 2. Add Items (fetching real prices from DB)
        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null) throw new Exception($"Product {item.ProductId} not found");

            order.AddItem(product.Id, product.Price, item.Quantity);
        }

        await _orderRepository.AddAsync(order);
        return _mapper.Map<OrderResponseDto>(order);
    }

    public async Task UpdateOrderAsync(int id, OrderUpdateDto dto)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder == null) throw new Exception("Order not found");

        existingOrder.SetStatus(dto.NewStatus);
        await _orderRepository.UpdateAsync(existingOrder);
    }

    public async Task UpdateOrderItemQuantityAsync(int orderId, int orderItemId, int quantity)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) throw new Exception("Order not found");
        order.UpdateItemQuantity(orderItemId, quantity);
        await _orderRepository.UpdateAsync(order);
    }

    public async Task SetShippingAddressForUserOrdersAsync(string userId, string address)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        foreach (var order in orders)
        {
            order.SetShippingAddress(address);
            await _orderRepository.UpdateAsync(order);
        }
    }

    public async Task SetShippingInfoForUserOrdersAsync(string userId, string address, string? phone)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        foreach (var order in orders)
        {
            order.SetShippingAddress(address);
            order.SetShippingPhone(phone);
            await _orderRepository.UpdateAsync(order);
        }
    }

    public async Task RemoveOrderItemAsync(int orderId, int orderItemId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) throw new Exception("Order not found");
        order.RemoveItem(orderItemId);
        await _orderRepository.UpdateAsync(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) throw new Exception("Order not found");

        await _orderRepository.DeleteAsync(order);
    }

    public async Task DeleteOrdersByUserIdAsync(string userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        foreach (var order in orders)
        {
            await _orderRepository.DeleteAsync(order);
        }
    }
}