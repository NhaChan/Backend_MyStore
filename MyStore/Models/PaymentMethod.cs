using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
