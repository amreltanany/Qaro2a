using ECommerce.API.Filters;
using ECommerce.Application.DTOs.Category;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [RequireAdmin]
    public class AdminCategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                await _categoryService.AddCategoryAsync(dto);
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (FluentValidation.ValidationException ex)
            {
                foreach (var err in ex.Errors)
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            var dto = new CategoryUpdateDto { Name = category.Name };
            ViewBag.Id = id;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryUpdateDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                await _categoryService.UpdateCategoryAsync(id, dto);
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (FluentValidation.ValidationException ex)
            {
                foreach (var err in ex.Errors)
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);
                ViewBag.Id = id;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                TempData["Success"] = "Category deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
