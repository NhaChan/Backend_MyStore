using MyStore.Enumerations;

namespace MyStore.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public double ShippingCost { get; set; }
        public double Total { get; set; }
        public double AmountPaid { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public DeliveryStatusEnum OrderStatus { get; set; }

    }
}
