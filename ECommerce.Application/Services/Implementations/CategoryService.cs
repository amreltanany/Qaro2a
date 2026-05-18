using AutoMapper;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Infrastructure.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryCreateDto> _createValidator;
        private readonly IMemoryCache _cache;
        private const string CategoryCacheKey = "AllCategories"; // Key used for cleanup caching

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IValidator<CategoryCreateDto> createValidator, IMemoryCache cache)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _createValidator = createValidator;
            _cache = cache;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            // Try to get categories from memory first (Cleanup performance)
            if (!_cache.TryGetValue(CategoryCacheKey, out IEnumerable<CategoryResponseDto> categories))
            {
                var categoryEntities = await _categoryRepository.GetAllAsync();
                categories = _mapper.Map<IEnumerable<CategoryResponseDto>>(categoryEntities);

                // Set cache to expire in 30 mins
                _cache.Set(CategoryCacheKey, categories, TimeSpan.FromMinutes(30));
            }

            return categories;
        }

        public async Task<CategoryResponseDto> AddCategoryAsync(CategoryCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new FluentValidation.ValidationException(validationResult.Errors);

            var category = _mapper.Map<Category>(dto);
            await _categoryRepository.AddAsync(category);

            // Invalidate Cache: Clear memory so the new category shows up
            _cache.Remove(CategoryCacheKey);

            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task UpdateCategoryAsync(int id, CategoryUpdateDto dto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Category {id} not found.");

            _mapper.Map(dto, existingCategory);
            await _categoryRepository.UpdateAsync(existingCategory);

            // Invalidate Cache: Data has changed
            _cache.Remove(CategoryCacheKey);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new Exception("Not found");

            await _categoryRepository.DeleteAsync(category);

            // Invalidate Cache: Item removed
            _cache.Remove(CategoryCacheKey);
        }

        public async Task<CategoryResponseDto> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return _mapper.Map<CategoryResponseDto>(category);
        }
    }
}