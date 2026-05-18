using ECommerce.Application.DTOs.Category;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto> GetCategoryByIdAsync(int id);
        Task<CategoryResponseDto> AddCategoryAsync(CategoryCreateDto dto);
        Task UpdateCategoryAsync(int id ,CategoryUpdateDto dto );
        Task DeleteCategoryAsync(int id);
    }
}
