using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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

        private Task<string?> GetUserIdFromCookieAsync()
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token)) return Task.FromResult<string?>(null);
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken;
            try { jwtToken = handler.ReadJwtToken(token); }
            catch { return Task.FromResult<string?>(null); }
            var userId = jwtToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.NameId)
                ?.Value;
            return Task.FromResult(userId);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdFromCookieAsync();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Home", new { sessionExpired = true });

            var cartItems = await _cartService.GetCartByUserIdAsync(userId);
            return View("/Views/Components/Cart.cshtml", cartItems);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = await GetUserIdFromCookieAsync();
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
            var userId = await GetUserIdFromCookieAsync();
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
