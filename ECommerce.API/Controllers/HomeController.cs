using ECommerce.API.Helpers;
using ECommerce.API.ViewModels;
using ECommerce.Application.DTOs.Query;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;
    private readonly IWishlistService _wishlistService;
    private readonly UserManager<User> _userManager;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IWishlistItemRepository _wishlistItemRepository;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        IOrderService orderService,
        IWishlistService wishlistService,
        UserManager<User> userManager,
        ICartItemRepository cartItemRepository,
        IWishlistItemRepository wishlistItemRepository)
    {
        _productService = productService;
        _categoryService = categoryService;
        _orderService = orderService;
        _wishlistService = wishlistService;
        _userManager = userManager;
        _cartItemRepository = cartItemRepository;
        _wishlistItemRepository = wishlistItemRepository;
    }

    private string? GetUserId() =>
        UserClaimsHelper.GetUserId(User)
        ?? UserClaimsHelper.GetUserIdFromJwtString(Request.Cookies["token"]);

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync(new QueryParameters());
        return View(products);
    }
    public async Task<IActionResult> Shop(
        string? s = null,
        DateTime? publishDate = null,
        string? author = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? categoryId = null,
        string? sortBy = null)
    {
        var query = new QueryParameters
        {
            PageNumber = 1,
            PageSize = 50,
            Search = string.IsNullOrWhiteSpace(s) ? null : s.Trim(),
            SortBy = string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim(),
            PublishDate = publishDate,
            Author = string.IsNullOrWhiteSpace(author) ? null : author.Trim(),
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            CategoryId = categoryId
        };
        var products = (await _productService.GetAllAsync(query)).ToList();
        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Search = query.Search;
        ViewBag.SortBy = query.SortBy;
        ViewBag.PublishDate = publishDate?.ToString("yyyy-MM-dd");
        ViewBag.Author = author;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.CategoryId = categoryId;
        return View("/Views/Components/Shop.cshtml", products);
    }
  
    public IActionResult Publish()
    {
        return RedirectToAction("Index", "Publish");
    }

    public IActionResult Blog()
    {
        return View("/Views/Components/Blog.cshtml");
    }

    public IActionResult Broadcast() => RedirectToActionPermanent(nameof(Podcast));

    public IActionResult Podcast()
    {
        return View("/Views/Components/Podcast.cshtml");
    }

    public IActionResult Contact()
    {
        return RedirectToAction("Index", "Contact");
    }

    public IActionResult About()
    {
        return View("/Views/Components/About.cshtml");
    }

    public async Task<IActionResult> Wishlist()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Home", new { sessionExpired = true });

        var wishlistItems = await _wishlistService.GetWishlistByUserIdAsync(userId);
        return View("/Views/Components/Wishlist.cshtml", wishlistItems);
    }

    public IActionResult Login()
    {
        return View("/Views/Components/Login.cshtml");
    }

    public IActionResult Register()
    {
        return View("/Views/Components/Register.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> MyAccount()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", new { sessionExpired = true });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction("Login", new { sessionExpired = true });

        var orders = (await _orderService.GetOrdersByUserIdAsync(userId)).ToList();
        var model = new MyAccountViewModel
        {
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Orders = orders,
            StatusMessage = TempData["StatusMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string
        };
        return View("/Views/Components/MyAccount.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string fullName, string? currentPassword, string? newPassword)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", new { sessionExpired = true });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction("Login", new { sessionExpired = true });

        if (string.IsNullOrWhiteSpace(fullName))
        {
            TempData["ErrorMessage"] = "Full name is required.";
            return RedirectToAction(nameof(MyAccount));
        }

        user.FullName = fullName.Trim();

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                TempData["ErrorMessage"] = "Enter your current password to set a new password.";
                return RedirectToAction(nameof(MyAccount));
            }

            var passwordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!passwordResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", passwordResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(MyAccount));
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Could not save profile changes.";
            return RedirectToAction(nameof(MyAccount));
        }

        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(MyAccount));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string deletePassword)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", new { sessionExpired = true });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction("Login", new { sessionExpired = true });

        if (!await _userManager.CheckPasswordAsync(user, deletePassword))
        {
            TempData["ErrorMessage"] = "Password is incorrect. Account was not deleted.";
            return RedirectToAction(nameof(MyAccount));
        }

        await _orderService.DeleteOrdersByUserIdAsync(userId);
        await _cartItemRepository.DeleteByUserIdAsync(userId);

        var wishlistItems = await _wishlistItemRepository.GetByUserIdAsync(userId);
        foreach (var item in wishlistItems)
            await _wishlistItemRepository.DeleteAsync(item);

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Could not delete account. Please try again.";
            return RedirectToAction(nameof(MyAccount));
        }

        Response.Cookies.Delete("token");
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Error()
    {
        return Content("Something went wrong. Please try again later.", "text/plain");
    }
}
