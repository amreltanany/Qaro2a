using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs.Product
{
   
        public class ProductResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string? ImageUrl { get; set; }
            public string Description { get; set; } = string.Empty;
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public DateTime PublishDate { get; set; }
            public string Author { get; set; } = string.Empty;
            public bool TopRated { get; set; }
        }
    
}
