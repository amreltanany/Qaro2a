using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

public class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        var normalizedCulture = string.Equals(culture, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(normalizedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/"
            });

        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return RedirectToAction("Index", "Home");

        return LocalRedirect(returnUrl);
    }
}
