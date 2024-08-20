namespace MyStore.Models
{
    public class Product : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
        public double Discount { get; set; }
        public int Quanlity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int CategoryId { get; set; }
        public Category Caterory { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        //public Type Type { get; set; }
        public ICollection<Image> Images { get; set; } = new HashSet<Image>();
    }
}
