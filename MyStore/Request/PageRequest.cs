namespace MyStore.Request
{
    public class PageRequest
    {
        public int page {  get; set; }
        public int pageSize { get; set; }
        public string? search { get; set; }
    }
}
