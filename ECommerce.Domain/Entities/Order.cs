using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string? UserId { get; private set; }
        public User? User { get; private set; }

        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public string? ShippingAddress { get; private set; }
        public string? ShippingPhone { get; private set; }
        public decimal DeliveryFee { get; private set; }

        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items;

        protected Order() { }

        public Order(string? userId, List<OrderItem> items)
        {
            UserId = userId;
            _items = items;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;
            DeliveryFee = OrderPricing.DeliveryFee;
        }

        public void AddItem(int productId, decimal price, int quantity)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero");

            _items.Add(new OrderItem(productId, price, quantity));
        }

        public decimal GetTotal()
            => _items.Sum(i => i.Price * i.Quantity) + DeliveryFee;

        public decimal GetItemsSubtotal()
            => _items.Sum(i => i.Price * i.Quantity);

        public void UpdateItemQuantity(int orderItemId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.Id == orderItemId);
            if (item == null)
                throw new InvalidOperationException("Order item not found");
            item.SetQuantity(quantity);
        }

        public void RemoveItem(int orderItemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == orderItemId);
            if (item != null)
                _items.Remove(item);
        }

        public void SetShippingAddress(string? address)
        {
            ShippingAddress = address;
        }

        public void SetShippingPhone(string? phone)
        {
            ShippingPhone = phone;
        }

        public void SetStatus(OrderStatus status)
        {
            Status = status;
        }
    }
}
