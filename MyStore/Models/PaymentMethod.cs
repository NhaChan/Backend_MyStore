using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class PaymentMethod
    {
        [Key]
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
