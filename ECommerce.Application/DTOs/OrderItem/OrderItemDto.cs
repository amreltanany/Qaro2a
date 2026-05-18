using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs.OrderItem
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        // This is "Flattening": We pull the Name from the Product entity
        // so the frontend doesn't have to do extra work.
        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // This is "Calculated Data": The UI doesn't want to do math.
        // We provide the subtotal directly.
        public decimal Subtotal => Price * Quantity;
    }
}
