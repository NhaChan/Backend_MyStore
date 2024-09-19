namespace MyStore.Request
{
    public class ProductRequest
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
        public double Discount { get; set; }
        public int Quantity { get; set; }
        public bool Enable { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public IEnumerable<string> ImageUrls { get; set; } = new List<string>();
    }
}
