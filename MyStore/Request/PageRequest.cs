namespace MyStore.Request
{
    public class PageRequest
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string? search { get; set; }
    }
}
