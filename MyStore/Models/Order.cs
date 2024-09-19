using MyStore.Enumerations;

namespace MyStore.Models
{
    public class Order : IBaseEntity
    {
        public int Id { get; set; }
        public double Total { get; set; }
        public double ShippingCost { get; set; }
        public DateTime OrderDate{ get; set; }
        public string ShippingAddress { get; set; }
        public string ReceiverInfo { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
        public string PaymentMethodName { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string OrderStatusName { get; set; } = DeliveryStatusEnum.Proccessing.ToString();
        public DeliveryStatus DeliveryStatus { get; set; }

    }
}
