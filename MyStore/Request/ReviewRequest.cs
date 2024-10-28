using System.ComponentModel.DataAnnotations;

namespace MyStore.Request
{
    public class ReviewRequest
    {
        public int ProductId { get; set; }

        [Range(0, 5)]
        public int Star { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
        public IFormFileCollection? Images { get; set; }
    }
}
