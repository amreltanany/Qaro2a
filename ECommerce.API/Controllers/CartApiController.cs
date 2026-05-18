using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/Cart")]
    public class CartApiController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartApiController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var items = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            try
            {
                var item = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);
                return Ok(item);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Insufficient stock", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int itemId, [FromBody] UpdateCartItemQuantityDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            try
            {
                await _cartService.UpdateCartItemQuantityAsync(itemId, userId, dto.Quantity);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Insufficient stock", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            await _cartService.RemoveCartItemAsync(itemId, userId);
            return NoContent();
        }
    }
}
