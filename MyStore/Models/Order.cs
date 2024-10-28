using MyStore.Enumerations;

namespace MyStore.Models
{
    public class Order : IBaseEntity
    {
        public long Id { get; set; }
        public double Total { get; set; }
        public double ShippingCost { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public bool Reviewed { get; set; }

        public string DeliveryAddress { get; set; }
        public string Receiver { get; set; }

        //GHN -> WardCode
        public string WardID { get; set; }
        public int DistrictID { get; set; }
        public string? ShippingCode { get; set; }
        public DateTime? Expected_delivery_time { get; set; }
        
        public double AmountPaid { get; set; }
        public string? PaymentTranId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        //public string OrderStatusName { get; set; } = DeliveryStatusEnum.Proccessing.ToString();
        public DeliveryStatusEnum? OrderStatus { get; set; } = DeliveryStatusEnum.Proccessing;

        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();

    }
}
