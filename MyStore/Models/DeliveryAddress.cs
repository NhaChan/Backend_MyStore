using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class DeliveryAddress : IBaseEntity
    {
        [Key]
        public string UserId { get; set; }
        public User User { get; set; }

        public string Name { get; set; }
        public string? PhoneNumber { get; set; }

        public int? ProvinceID { get; set; }
        public string? ProvinceName { get; set; }

        public int? DistrictID { get; set; }
        public string? DistrictName { get; set; }

        public int? WardID { get; set; }
        public string? WardName { get; set; }

        public string? Detail { get; set;}

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
