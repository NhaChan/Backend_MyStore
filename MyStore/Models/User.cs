using Microsoft.AspNetCore.Identity;


namespace MyStore.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }
        public ICollection<Coupon> Coupons { get; set; }
    }
}
