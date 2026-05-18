using ECommerce.API.Helpers;
using ECommerce.Application.DTOs.Query;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;
    private readonly IWishlistService _wishlistService;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        IOrderService orderService,
        IWishlistService wishlistService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _orderService = orderService;
        _wishlistService = wishlistService;
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

    public IActionResult Broadcast()
    {
        return View("/Views/Components/Broadcast.cshtml");
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
}
