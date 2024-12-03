namespace MyStore.Request
{
    public class StockReceiptProduct
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double OriginPrice { get; set; }
    }
    public class StockReceiptRequest
    {
        public double Total { get; set; }
        public string? Note { get; set; }
        public DateTime EntryDate { get; set; }
        public IEnumerable<StockReceiptProduct> StockReceiptProducts { get; set; }
    }
}
