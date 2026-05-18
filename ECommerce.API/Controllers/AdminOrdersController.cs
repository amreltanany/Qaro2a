using ECommerce.API.Filters;
using ECommerce.Application.DTOs.Order;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [RequireAdmin]
    public class AdminOrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                await _orderService.UpdateOrderAsync(id, new OrderUpdateDto { OrderId = id, NewStatus = newStatus });
                TempData["Success"] = "Order status updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to update order status.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
