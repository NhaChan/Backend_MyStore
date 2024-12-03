using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Product : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string? Description { get; set; }
        public double Discount { get; set; }
        public int Quantity { get; set; }
        public bool Enable { get; set; }
        [Range(0, int.MaxValue)]
        public int Sold { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int CategoryId { get; set; }
        public Category Caterory { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }

        [Range(0,5)]
        public float Rating { get; set; }
        public long RatingCount { get; set; }
        //public Type Type { get; set; }
        public ICollection<Image> Images { get; set; } = new HashSet<Image>();
        public ICollection<ProductReview> ProductReviews { get; } = new HashSet<ProductReview>();
        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();
        public ICollection<ProductFavorite> ProductFavorites { get; } = new HashSet<ProductFavorite>();

    }
}
