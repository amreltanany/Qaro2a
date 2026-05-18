using ECommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public DateTime PublishDate { get; private set; }
        public string Author { get; private set; }
        public bool TopRated { get; private set; }

        public string? ImageUrl { get; set; }
        public string Description { get; private set; }

        //CategoryId → foreign key, simple integer to link to Category in the database
        public int CategoryId { get; private set; }

        //optional navigation property, lets EF Core load the full Category if needed
        
        public Category? Category { get; private set; }

        protected Product() { }

        public Product(string name, decimal price, int stock, string description, int categoryId)
        {
            Name = name;
            Price = price;
            Stock = stock;
            CategoryId = categoryId;
            Description = description;
            PublishDate = DateTime.UtcNow;
            Author = string.Empty;
            TopRated = false;
        }

        public void ReduceStock(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero");

            if (quantity > Stock)
                throw new InvalidOperationException("Insufficient stock");

            Stock -= quantity;
        }

        public void SetStock(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Stock cannot be negative.");
            Stock = value;
        }

        /// <summary>
        /// Restore stock (e.g. when item removed from cart or quantity decreased).
        /// </summary>
        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero");
            Stock += quantity;
        }
    }
}
