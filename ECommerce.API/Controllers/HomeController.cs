using ECommerce.API.Helpers;
using ECommerce.API.ViewModels;
using ECommerce.Application.DTOs.Query;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;
    private readonly IWishlistService _wishlistService;
    private readonly UserManager<User> _userManager;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IWishlistItemRepository _wishlistItemRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        IOrderService orderService,
        IWishlistService wishlistService,
        UserManager<User> userManager,
        ICartItemRepository cartItemRepository,
        IWishlistItemRepository wishlistItemRepository,
        ILogger<HomeController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _orderService = orderService;
        _wishlistService = wishlistService;
        _userManager = userManager;
        _cartItemRepository = cartItemRepository;
        _wishlistItemRepository = wishlistItemRepository;
        _logger = logger;
    }

    private string? GetUserId() =>
        UserClaimsHelper.GetUserId(User)
        ?? UserClaimsHelper.GetUserIdFromJwtString(Request.Cookies["token"]);

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync(new QueryParameters
        {
            PageNumber = 1,
            PageSize = 3,
            TopRatedOnly = true,
            SortBy = "publishdate_desc"
        });
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
        var priciest = (await _productService.GetAllAsync(new QueryParameters { PageNumber = 1, PageSize = 1, SortBy = "price_desc" })).FirstOrDefault();
        ViewBag.PriceCeil = priciest == null ? 1000 : (int)(Math.Ceiling(priciest.Price / 50m) * 50m);
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
  
    // /Home/Author/طاهرة-مافي  (spaces are written as hyphens in the URL)
    [HttpGet("Home/Author/{name}")]
    public Task<IActionResult> Author(string name)
    {
        var author = Uri.UnescapeDataString(name ?? string.Empty).Replace('-', ' ').Trim();
        return Shop(author: author);
    }

    // /Home/Category/جريمة-وغموض-وإثارة  (spaces are written as hyphens in the URL)
    [HttpGet("Home/Category/{name}")]
    public async Task<IActionResult> Category(string name)
    {
        var slug = Uri.UnescapeDataString(name ?? string.Empty).Trim();
        var category = (await _categoryService.GetAllCategoriesAsync())
            .FirstOrDefault(c => c.Name.Trim().Replace(' ', '-') == slug || c.Name.Trim() == slug);
        if (category == null) return NotFound();
        return await Shop(categoryId: category.Id);
    }

    [HttpGet("Home/Product/{id:int}/{slug?}")]
    public async Task<IActionResult> Product(int id, string? slug = null)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

        // canonical URL: /Home/Product/{id}/{name-with-hyphens}
        var canonical = ECommerce.API.Helpers.SlugHelper.ProductPath(product.Id, product.Name);
        var wanted = ECommerce.API.Helpers.SlugHelper.Slug(product.Name);
        if (!string.Equals(slug ?? string.Empty, wanted, StringComparison.Ordinal))
            return RedirectPermanent(canonical + Request.QueryString);

        var related = (await _productService.GetAllAsync(new QueryParameters
        {
            PageNumber = 1,
            PageSize = 5,
            CategoryId = product.CategoryId
        })).Where(p => p.Id != product.Id).Take(4).ToList();

        ViewBag.Related = related;
        return View("/Views/Components/ProductDetails.cshtml", product);
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

    public IActionResult PrivacyPolicy() => View("/Views/Components/PrivacyPolicy.cshtml");

    public IActionResult TermsOfService() => View("/Views/Components/TermsOfService.cshtml");

    public IActionResult Register()
    {
        return View("/Views/Components/Register.cshtml");
    }

    public IActionResult ForgotPassword() => RedirectToAction(nameof(Login));

    public IActionResult ResetPassword(string? email) => RedirectToAction(nameof(Login));

    [HttpGet]
    public async Task<IActionResult> MyAccount()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", new { sessionExpired = true });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction("Login", new { sessionExpired = true });

        var orders = new List<ECommerce.Application.DTOs.Order.OrderResponseDto>();
        string? ordersError = null;
        try
        {
            orders = (await _orderService.GetOrdersByUserIdAsync(userId)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load orders for user {UserId}", userId);
            ordersError = "Order history could not be loaded. The site database may need a pending update—please try again later.";
        }

        var model = new MyAccountViewModel
        {
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Orders = orders,
            StatusMessage = TempData["StatusMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string ?? ordersError
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

    // Re-executed by the status-code middleware for 403/404/... on page requests.
    [HttpGet]
    public IActionResult StatusPage(int code = 404)
    {
        ViewData["Code"] = code;
        return View("/Views/Components/StatusPage.cshtml");
    }

    [HttpGet]
    public IActionResult Error()
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View("/Views/Home/Error.cshtml");
    }
}
