using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Wishlist;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/Wishlist")]
    public class WishlistApiController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistApiController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyWishlist()
        {
            var userId = UserClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var items = await _wishlistService.GetWishlistByUserIdAsync(userId);
            return Ok(items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto dto)
        {
            var userId = UserClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var item = await _wishlistService.AddToWishlistAsync(userId, dto.ProductId);
            return Ok(item);
        }

        [Authorize]
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var userId = UserClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            await _wishlistService.RemoveWishlistItemAsync(itemId, userId);
            return NoContent();
        }
    }
}
