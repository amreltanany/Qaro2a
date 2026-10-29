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

        // cart owner: the signed-in user, otherwise the guest cart (a signed-in user inherits the guest cart)
        private async Task<(string? Key, string? UserId)> ResolveOwnerAsync()
        {
            var userId = ResolveUserId();
            var guest = CartOwner.GuestKey(HttpContext, false);
            if (!string.IsNullOrEmpty(userId))
            {
                if (guest != null)
                {
                    await _cartService.MergeGuestCartAsync(guest, userId);
                    CartOwner.ClearGuest(HttpContext);
                }
                return (userId, userId);
            }
            return (guest, null);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (key, _) = await ResolveOwnerAsync();
            if (string.IsNullOrEmpty(key))
                return View("/Views/Components/Cart.cshtml", Enumerable.Empty<ECommerce.Application.DTOs.Cart.CartItemDto>());

            try
            {
                var cartItems = await _cartService.GetCartByUserIdAsync(key);
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
            var (key, userId) = await ResolveOwnerAsync();
            if (string.IsNullOrEmpty(key))
                return RedirectToAction("Index");

            var cartItems = (await _cartService.GetCartByUserIdAsync(key)).ToList();
            if (cartItems.Count == 0)
                return RedirectToAction("Index");

            User? user = userId == null ? null : await _userManager.FindByIdAsync(userId);
            if (userId != null && user == null)
                return RedirectToAction("Login", "Home", new { sessionExpired = true });

            return View("/Views/Components/Checkout.cshtml", new CheckoutViewModel
            {
                IsGuest = user == null,
                FullName = user?.FullName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                Address = string.Empty,
                CartItems = cartItems
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(string FullName, string Email, string Phone, string Address)
        {
            var (key, userId) = await ResolveOwnerAsync();
            if (string.IsNullOrEmpty(key))
                return RedirectToAction("Index");

            User? user = userId == null ? null : await _userManager.FindByIdAsync(userId);
            if (userId != null && user == null)
                return RedirectToAction("Login", "Home", new { sessionExpired = true });
            var isGuest = user == null;

            var missing = string.IsNullOrWhiteSpace(Address)
                || (isGuest && (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Phone)));
            if (missing)
            {
                var cartItems = (await _cartService.GetCartByUserIdAsync(key)).ToList();
                return View("/Views/Components/Checkout.cshtml", new CheckoutViewModel
                {
                    IsGuest = isGuest,
                    FullName = user?.FullName ?? FullName ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    Phone = user?.PhoneNumber ?? Phone ?? string.Empty,
                    Address = Address ?? string.Empty,
                    CartItems = cartItems
                });
            }

            await _cartService.CreateOrderFromCartAsync(
                key,
                userId,
                Address.Trim(),
                Phone?.Trim(),
                isGuest ? FullName.Trim() : user!.FullName);
            TempData["OrderPlaced"] = true;
            return RedirectToAction("Index", "Home");
        }
    }
}
