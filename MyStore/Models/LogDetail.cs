namespace MyStore.Models
{
    public class LogDetail
    {
        public int Id { get; set; }
        public long LogId { get; set; }
        public Log Log { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double OriginPrice { get; set; }
    }
}
