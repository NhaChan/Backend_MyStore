using Microsoft.AspNetCore.Identity;


namespace MyStore.Models
{
    public class User : IdentityUser, IBaseEntity
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
