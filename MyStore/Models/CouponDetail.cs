namespace MyStore.Models
{
    public class CouponDetail : IBaseEntity
    {
        public int Id { get; set; }
        public int Quanlity { get; set; }
        public double Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<CouponDetail> Coupons { get;}
        public ICollection<Product> Products { get;}

    }
}
