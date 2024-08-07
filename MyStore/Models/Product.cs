using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Product : IBaseEntity
    {
        [Key]
        public long ProductID { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public int Quanlity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public Category Caterory { get; set; }
        public Brand Brand { get; set; }
        public Type Type { get; set; }



    }
}
