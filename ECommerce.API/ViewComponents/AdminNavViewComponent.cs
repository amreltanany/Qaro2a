using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.ViewComponents
{
    public class AdminNavViewComponent : ViewComponent
    {
        private const string AdminEmail = "amr_eltanany@outlook.com";

        /// <summary>
        /// Only show "Welcome admin" and "Dashboard" when the user is logged in with Identity cookie AND is the admin (AdminEmail).
        /// When not logged in or logged in as non-admin, nothing is rendered.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(string section = "icons")
        {
            var result = await HttpContext.AuthenticateAsync("Identity.Application");
            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var isAdmin = result.Succeeded
                && !string.IsNullOrWhiteSpace(email)
                && email.Trim().Equals(AdminEmail, StringComparison.OrdinalIgnoreCase);
            return section == "main" ? View("MainNav", isAdmin) : View(isAdmin);
        }
    }
}
