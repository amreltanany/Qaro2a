using ECommerce.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Order
{
    public class OrderUpdateDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "A status must be provided.")]
        public OrderStatus NewStatus { get; set; }
        public string? AdminNotes { get; set; }
    }
}
