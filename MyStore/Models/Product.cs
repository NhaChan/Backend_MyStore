using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Product
    {
        [Key]
        public long ProductID { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public int Quanlity { get; set; }
        public Caterory Caterory { get; set; }
        public Brand Brand { get; set; }
        public Type Type { get; set; }



    }
}
