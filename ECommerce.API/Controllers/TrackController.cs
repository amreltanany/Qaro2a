using ECommerce.API.ViewModels;
using ECommerce.Application.DTOs.Order;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

/// <summary>Order tracking for people without an account: order number + the phone number used on the order.</summary>
public class TrackController : Controller
{
    private const string View_ = "/Views/Components/TrackOrder.cshtml";
    private readonly IOrderService _orderService;
    private readonly ILogger<TrackController> _logger;

    public TrackController(IOrderService orderService, ILogger<TrackController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet("Track")]
    public IActionResult Index() => View(View_, new TrackViewModel());

    [HttpPost("Track")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("track")]
    public async Task<IActionResult> Lookup(string? orderNumber, string? phone)
    {
        var model = new TrackViewModel { OrderNumber = Clip(orderNumber), Phone = Clip(phone) };
        if (!int.TryParse(model.OrderNumber?.TrimStart('#', ' '), out var id) || id <= 0 || string.IsNullOrWhiteSpace(phone))
        {
            model.NotFound = true;
            return View(View_, model);
        }

        var order = await FindAsync(id);
        // the same answer for "no such order" and "wrong phone", so numbers cannot be probed
        if (order == null || !PhonesMatch(order.ShippingPhone, phone))
        {
            _logger.LogInformation("Order tracking miss for order {OrderId}", id);
            model.NotFound = true;
            return View(View_, model);
        }
        model.Order = order;
        return View(View_, model);
    }

    /// <summary>Shown right after checkout. The order id travels in TempData (one-time, encrypted cookie), not in the URL.</summary>
    [HttpGet("Track/Placed")]
    public async Task<IActionResult> Placed()
    {
        if (TempData["PlacedOrderId"] is not int id)
            return RedirectToAction(nameof(Index));
        var order = await FindAsync(id);
        if (order == null) return RedirectToAction(nameof(Index));
        return View(View_, new TrackViewModel { Order = order, JustPlaced = true });
    }

    private async Task<OrderResponseDto?> FindAsync(int id)
    {
        try { return await _orderService.GetOrderByIdAsync(id); }
        catch (Exception) { return null; } // the service throws when the order does not exist
    }

    private static string? Clip(string? v) => string.IsNullOrWhiteSpace(v) ? null : (v.Trim().Length > 30 ? v.Trim()[..30] : v.Trim());

    private static string Norm(string? p)
    {
        var d = new string((p ?? string.Empty).Where(char.IsDigit).ToArray());
        if (d.StartsWith("0020")) d = d[4..];
        else if (d.StartsWith("20") && d.Length >= 11) d = d[2..];
        return d.TrimStart('0');
    }

    private static bool PhonesMatch(string? stored, string? typed)
    {
        var a = Norm(stored);
        return a.Length >= 7 && a == Norm(typed);
    }
}
