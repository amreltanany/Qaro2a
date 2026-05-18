using ECommerce.API.Helpers;
using ECommerce.API.ViewModels;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;

        public CartController(ICartService cartService, UserManager<User> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        private string? ResolveUserId() =>
            UserClaimsHelper.GetUserId(User)
            ?? UserClaimsHelper.GetUserIdFromJwtString(Request.Cookies["token"]);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = ResolveUserId();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Home", new { sessionExpired = true });

            try
            {
                var cartItems = await _cartService.GetCartByUserIdAsync(userId);
                return View("/Views/Components/Cart.cshtml", cartItems);
            }
            catch (Exception)
            {
                TempData["CartError"] = "Unable to load your cart. Data may be out of sync — please try again after we refresh.";
                return View("/Views/Components/Cart.cshtml", Enumerable.Empty<ECommerce.Application.DTOs.Cart.CartItemDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = ResolveUserId();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Home", new { sessionExpired = true });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Home", new { sessionExpired = true });

            var cartItems = (await _cartService.GetCartByUserIdAsync(userId)).ToList();
            if (cartItems.Count == 0)
                return RedirectToAction("Index");

            var model = new CheckoutViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                Address = string.Empty,
                CartItems = cartItems
            };
            return View("/Views/Components/Checkout.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(string FullName, string Email, string Phone, string Address)
        {
            var userId = ResolveUserId();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Home", new { sessionExpired = true });

            if (string.IsNullOrWhiteSpace(Address))
            {
                ModelState.AddModelError("Address", "Address is required.");
                var user = await _userManager.FindByIdAsync(userId);
                var cartItems = (await _cartService.GetCartByUserIdAsync(userId)).ToList();
                return View("/Views/Components/Checkout.cshtml", new CheckoutViewModel
                {
                    FullName = user?.FullName ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    Phone = user?.PhoneNumber ?? string.Empty,
                    Address = Address ?? string.Empty,
                    CartItems = cartItems
                });
            }

            await _cartService.CreateOrderFromCartAsync(userId, Address.Trim(), Phone?.Trim());
            TempData["OrderPlaced"] = true;
            return RedirectToAction("Index", "Home");
        }
    }
}
