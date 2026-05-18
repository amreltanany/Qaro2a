using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.DTOs.Query;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        // Updated for cleanup pagination and filtering
        Task<IEnumerable<ProductResponseDto>> GetAllAsync(QueryParameters queryParams);
        Task<ProductResponseDto> GetByIdAsync(int id);
        Task<ProductResponseDto> AddAsync(ProductCreateDto dto);
        Task UpdateAsync(int id, ProductUpdateDto dto);
        Task DeleteAsync(int id);
    }
}