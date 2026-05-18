using ECommerce.Application.DTOs.OrderItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs.Order
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }

        public DateTime OrderDate { get; set; }
        public string Status { get; set; } // Convert Enum to String for the UI
        public string? ShippingAddress { get; set; }
        public string? ShippingPhone { get; set; }

        public decimal TotalAmount { get; set; } // The result of your GetTotal() method
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
