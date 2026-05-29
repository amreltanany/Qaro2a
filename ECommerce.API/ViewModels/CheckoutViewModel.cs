using ECommerce.Application.DTOs.Cart;
using ECommerce.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace ECommerce.API.ViewModels
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public IEnumerable<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
        public decimal ItemsSubtotal => CartItems.Sum(c => c.Subtotal);
        public decimal DeliveryFee => OrderPricing.DeliveryFee;
        public decimal GrandTotal => ItemsSubtotal + DeliveryFee;
    }
}
