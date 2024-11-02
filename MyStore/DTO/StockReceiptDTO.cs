namespace MyStore.DTO
{
    public class StockReceiptDTO
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string? Note { get; set; }
        public double Total { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
