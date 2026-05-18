using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs.Category
{
        public class CategoryResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;

            // Optional: Count of products can be useful for UI sidebars
            
        }
    }
