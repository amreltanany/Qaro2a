using ECommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public  class Category : BaseEntity
    {
        public string Name { get; private set; }

        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products;

        protected Category() { } // For ORM later

        public Category(string name)
        {
            Name = name;
        }
    }
}
