using System.Security.Claims;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.OrderItem;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderDto)
        {
            var createdOrder = await _orderService.AddOrderAsync(orderDto);
            return CreatedAtAction(nameof(Get), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, OrderUpdateDto orderDto)
        {
            await _orderService.UpdateOrderAsync(id, orderDto);
            return NoContent();
        }

        [Authorize]
        [HttpPut("{orderId}/items/{itemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int orderId, int itemId, [FromBody] UpdateOrderItemQuantityDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            await _orderService.UpdateOrderItemQuantityAsync(orderId, itemId, dto.Quantity);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{orderId}/items/{itemId}")]
        public async Task<IActionResult> RemoveOrderItem(int orderId, int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            await _orderService.RemoveOrderItemAsync(orderId, itemId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}