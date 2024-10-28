namespace MyStore.Response
{
    public class StockReceiptDetailResponse
    {
        public long Id { get; set; }
        public int Quantity { get; set; }
        public double OriginPrice { get; set; }
        
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
