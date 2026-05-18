namespace ECommerce.Application.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        /// <summary>Current product stock (max quantity user can have in cart for this item).</summary>
        public int AvailableStock { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}
