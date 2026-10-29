using ECommerce.API.Helpers;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.ViewComponents
{
    /// <summary>Number of items in the visitor's cart (signed-in user or guest cookie).</summary>
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ICartItemRepository _cart;

        public CartCountViewComponent(ICartItemRepository cart) => _cart = cart;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var key = UserClaimsHelper.GetUserId(HttpContext.User)
                ?? UserClaimsHelper.GetUserIdFromJwtString(Request.Cookies["token"])
                ?? CartOwner.GuestKey(HttpContext, false);
            var count = string.IsNullOrEmpty(key) ? 0 : await _cart.CountQuantityAsync(key);
            return Content(count.ToString());
        }
    }
}
