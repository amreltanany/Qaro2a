using ECommerce.API.Filters;
using ECommerce.API.Helpers;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public DashboardController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Called after main site login when user is admin. Sets Identity cookie so nav shows Dashboard and user can access dashboard.
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IActionResult> SetAdminCookie()
        {
            var email = UserClaimsHelper.GetEmail(User);
            if (!UserClaimsHelper.IsAdminEmail(email))
                return Forbid();
            var user = await _userManager.FindByEmailAsync(email!.Trim());
            if (user == null) return Forbid();
            await _signInManager.SignInAsync(user, isPersistent: true);
            return Json(new { redirect = Url.Action(nameof(Index), "Dashboard") });
        }

        [RequireAdmin]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
