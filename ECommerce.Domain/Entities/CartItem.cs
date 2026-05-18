using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public string UserId { get; private set; } = string.Empty;
        public User? User { get; private set; }
        public int ProductId { get; private set; }
        public Product? Product { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }

        protected CartItem() { }

        public CartItem(string userId, int productId, decimal price, int quantity)
        {
            UserId = userId;
            ProductId = productId;
            Price = price;
            Quantity = quantity;
        }

        public void SetQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero");
            Quantity = quantity;
        }

        public decimal Subtotal => Price * Quantity;
    }
}
