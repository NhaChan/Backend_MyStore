using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Brand : IBaseEntity
    {
        [Key]
        public long BrandID { get; set; }
        public string BrandName { get; set; }
        public string Picture { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
