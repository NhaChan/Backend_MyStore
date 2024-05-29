using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Product
    {
        [Key]
        public long ProductID { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public DateTime? DateOfPurchase { get; set; }
        public string AvailabilityStatus { get; set; }
        public long? BrandID { get; set; }
        public long? CategoryID { get; set; }
        public bool? Avtive { get; set; } 
        public virtual Brand Brand { get; set; }
        public virtual Caterory Caterory { get; set; }



    }
}
