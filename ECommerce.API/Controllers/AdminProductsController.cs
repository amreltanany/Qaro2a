using ECommerce.API.Filters;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.DTOs.Query;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [RequireAdmin]
    public class AdminProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;

        public AdminProductsController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, CancellationToken cancellationToken = default)
        {
            var query = new QueryParameters { PageNumber = page, PageSize = 10, Search = search };
            var products = await _productService.GetAllAsync(query);
            ViewBag.Search = search;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(new ProductCreateDto { Stock = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateDto dto, IFormFile? image, CancellationToken cancellationToken = default)
        {
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("Image", "Product image is required when creating a product.");
            }
            else if (!image.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("Image", "Please upload a valid image file.");
            }
            else
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                await using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream, cancellationToken);
                dto.ImageUrl = $"/images/products/{fileName}";
            }
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(dto);
            }
            try
            {
                await _productService.AddAsync(dto);
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (FluentValidation.ValidationException ex)
            {
                foreach (var err in ex.Errors)
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            var dto = new ProductUpdateDto
            {
                id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                Stock = product.Stock,
                PublishDate = product.PublishDate,
                Author = product.Author,
                TopRated = product.TopRated
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpdateDto dto, IFormFile? image, CancellationToken cancellationToken = default)
        {
            if (id != dto.id) return BadRequest();
            if (image != null && image.Length > 0 && image.ContentType.StartsWith("image/"))
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                await using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream, cancellationToken);
                dto.ImageUrl = $"/images/products/{fileName}";
            }
            try
            {
                await _productService.UpdateAsync(id, dto);
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (FluentValidation.ValidationException ex)
            {
                foreach (var err in ex.Errors)
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
        {
            await _productService.DeleteAsync(id);
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
