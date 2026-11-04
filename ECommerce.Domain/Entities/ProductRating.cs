using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    /// <summary>One visitor's 1-5 star rating of one book. OwnerKey is the user id or the guest cookie key, so each visitor counts once.</summary>
    public class ProductRating : BaseEntity
    {
        public int ProductId { get; private set; }
        public Product? Product { get; private set; }
        public string OwnerKey { get; private set; } = string.Empty;
        public int Stars { get; private set; }

        protected ProductRating() { }

        public ProductRating(int productId, string ownerKey, int stars)
        {
            ProductId = productId;
            OwnerKey = ownerKey;
            Stars = Math.Clamp(stars, 1, 5);
        }

        public void SetStars(int stars) => Stars = Math.Clamp(stars, 1, 5);
    }
}
