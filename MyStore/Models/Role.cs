using Microsoft.AspNetCore.Identity;

namespace MyStore.Models
{
    public class Role : IdentityRole
    {
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
