using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Enums
{
    public enum OrderStatus
    //An enum lets you say: this value can ONLY be one of these options
    {
        Pending,
        Paid,
        Shipped,
        Cancelled
    }
}
