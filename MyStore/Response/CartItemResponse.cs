namespace MyStore.Response
{
    public class CartItemResponse
    {
        public string Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public int? Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
