namespace MyStore.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
        public double Discount { get; set; }
        public int Quanlity { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string ImageUrl { get; set; }
    }
}
