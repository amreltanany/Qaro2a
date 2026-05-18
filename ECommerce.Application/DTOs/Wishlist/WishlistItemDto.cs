namespace ECommerce.Application.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string Description { get; set; } = string.Empty;
        public int AvailableStock { get; set; }
    }
}
