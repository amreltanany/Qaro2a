using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Common
{
    //All entities will inherit from this.
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }
        // CHANGE THIS: CreateAt -> CreatedAt
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }
}
