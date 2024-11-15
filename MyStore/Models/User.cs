using Microsoft.AspNetCore.Identity;


namespace MyStore.Models
{
    public class User : IdentityUser, IBaseEntity
    {
        public string? FullName { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public DeliveryAddress? DeliveryAddress { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
