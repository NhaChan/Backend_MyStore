using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    //[PrimaryKey(nameof(UserId), nameof(ProductId))]
    public class CartItem : IBaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
