using ECommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    //it’s the link between Order and Product
    public class OrderItem : BaseEntity
    {
        public int ProductId { get; private set; }
        public Product? Product { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }

        protected OrderItem() { }

        public OrderItem(int productId, decimal price, int quantity)
        {
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
    }
}
