using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs.Product
{

    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be 0 or greater")]
        public int Stock { get; set; }
       
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [MaxLength(2048, ErrorMessage = "Image URL cannot exceed 2048 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Publish date is required")]
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Author is required")]
        [MaxLength(100, ErrorMessage = "Author cannot exceed 100 characters")]
        public string Author { get; set; } = string.Empty;

        public bool TopRated { get; set; }
    }

}
