using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Helpers
{
    public static class CartOwner
    {
        public const string GuestCookie = "qaro2a_guest";
        public const string GuestPrefix = "guest:";

        public static string? GuestKey(HttpContext http, bool create)
        {
            var raw = http.Request.Cookies[GuestCookie];
            if (!string.IsNullOrEmpty(raw) && Guid.TryParse(raw, out var existing))
                return GuestPrefix + existing.ToString("N");
            if (!create) return null;

            var id = Guid.NewGuid();
            http.Response.Cookies.Append(GuestCookie, id.ToString("N"), new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = http.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                IsEssential = true
            });
            return GuestPrefix + id.ToString("N");
        }

        public static void ClearGuest(HttpContext http) => http.Response.Cookies.Delete(GuestCookie);
    }
}
