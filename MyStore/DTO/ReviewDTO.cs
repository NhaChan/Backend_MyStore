namespace MyStore.DTO
{
    public class ReviewDTO
    {
        public string Id { get; set; }
        public string? Description { get; set; }
        public int Star { get; set; }
        public string Username { get; set; }
        public List<string>? ImagesUrls { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public float Rating { get; set; }
        public long RatingCount { get; set; }
    }
}
