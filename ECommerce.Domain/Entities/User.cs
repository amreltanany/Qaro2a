using ECommerce.Domain.Common;
using Microsoft.AspNetCore.Identity;
namespace ECommerce.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
