using ECommerce.Application.DTOs.Order;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(string userId);
        Task<OrderResponseDto> GetOrderByIdAsync(int id);
        Task<OrderResponseDto> AddOrderAsync(OrderCreateDto dto);
        Task UpdateOrderAsync(int id ,OrderUpdateDto dto);
        Task UpdateOrderItemQuantityAsync(int orderId, int orderItemId, int quantity);
        Task SetShippingAddressForUserOrdersAsync(string userId, string address);
        Task SetShippingInfoForUserOrdersAsync(string userId, string address, string? phone);
        Task RemoveOrderItemAsync(int orderId, int orderItemId);
        Task DeleteOrderAsync(int id);
        Task DeleteOrdersByUserIdAsync(string userId);
    }
}
