using MyStore.Enumerations;

namespace MyStore.Request
{
    public class OrderStatusRequest
    {
        public DeliveryStatusEnum Status { get; set; }
    }
}
