using AutoMapper;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.DTOs.Query;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using FluentValidation;

namespace ECommerce.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductCreateDto> _createValidator;
        private readonly IValidator<ProductUpdateDto> _updateValidator;

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            IValidator<ProductCreateDto> createValidator, 
            IValidator<ProductUpdateDto> updateValidator)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync(QueryParameters queryParams)
        {
            var products = await _productRepository.GetAllAsync(queryParams);

            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        public async Task<ProductResponseDto> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null) return null;

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> AddAsync(ProductCreateDto dto)
        {
            // The call that is currently crashing in image_b3370a.png
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                // Explicitly use FluentValidation's exception
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            var product = _mapper.Map<Product>(dto);
            await _productRepository.AddAsync(product);

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task UpdateAsync(int id, ProductUpdateDto dto)
        {
            // 1. Validate the DTO before doing anything else
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            // 2. Business Logic: Ensure the IDs match
            if (id != dto.id)
            {
                throw new ValidationException("The ID in the URL does not match the ID in the request body.");
            }

            // 3. Check if it exists
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            // 4. Map and Save
            _mapper.Map(dto, existingProduct);
            existingProduct.SetStock(dto.Stock);
            await _productRepository.UpdateAsync(existingProduct);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            await _productRepository.DeleteAsync(product);
        }
    }
}