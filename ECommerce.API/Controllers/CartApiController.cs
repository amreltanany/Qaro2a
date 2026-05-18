using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.NameId)
            ?? User.FindFirstValue("sub");

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var items = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            try
            {
                var item = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Insufficient stock", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Could not save cart item. Check that database migrations are applied (CartItem table)." });
            }
        }

        [Authorize]
        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int itemId, [FromBody] UpdateCartItemQuantityDto dto)
        {
            var userId = GetUserId();
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
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            await _cartService.RemoveCartItemAsync(itemId, userId);
            return NoContent();
        }
    }
}
