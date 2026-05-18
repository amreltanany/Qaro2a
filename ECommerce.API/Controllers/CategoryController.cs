using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services.Implementations;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Get all categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // Get category by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponseDto>> GetById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        // Create a new category
        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> Create(CategoryCreateDto dto)
        {
            try
            {
                var created = await _categoryService.AddCategoryAsync(dto);
                //It returns a 201 Created status code with the new product's data in the body
                // adds a Location header URL pointing to where that specific product can be found.
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); 

            }
            catch (FluentValidation.ValidationException ex)
            {
                // Returns 400 Bad Request if validation fails
                return BadRequest(ex.Errors);
            }
        }

        // Update a category
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
        {
            try
            {
                await _categoryService.UpdateCategoryAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Category not found." });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        // Delete a category
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
