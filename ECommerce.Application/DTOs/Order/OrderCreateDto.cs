using ECommerce.Application.DTOs.OrderItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs.Order
{
    
        public class OrderCreateDto // Add 'public'
        {
        /// <summary>Optional. Omit or use "guest" for guest checkout.</summary>
        public string? UserId { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
        }
    }

