using ECommerce.Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs.Category
{
    namespace ECommerce.Application.DTOs
    {
        public class CategoryDetailResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;

            // We use the Product DTO here if the ui need to see the product list 
            public List<ProductResponseDto> Products { get; set; } = new();
        }
    }
}
