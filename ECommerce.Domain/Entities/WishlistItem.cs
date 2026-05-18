using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class WishlistItem : BaseEntity
    {
        public string UserId { get; private set; } = string.Empty;
        public User? User { get; private set; }
        public int ProductId { get; private set; }
        public Product? Product { get; private set; }

        protected WishlistItem() { }

        public WishlistItem(string userId, int productId)
        {
            UserId = userId;
            ProductId = productId;
        }
    }
}
