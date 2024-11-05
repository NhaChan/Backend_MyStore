using MyStore.Enumerations;

namespace MyStore.DTO
{
    public class OrderDTO
    {
        public long Id { get; set; }
        public double ShippingCost { get; set; }
        public double Total { get; set; }
        public double AmountPaid { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodName { get; set; }

        public DeliveryStatusEnum OrderStatus { get; set; }

        public string? ShippingCode { get; set; }
        public DateTime? Expected_delivery_time { get; set; }
        public string? PayBackUrl { get; set; }

        public ProductDTO Product { get; set; }
        public bool Reviewed { get; set; }
        public DateTime DateReceived { get; set; }

    }

    //public class OrderDetailsResponse : OrderDTO
    //{
    //    public IEnumerable<ProductOrderDetails> ProductOrderDetails { get; set; }
    //}

    public class ProductOrderDetails
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; }

        public double OriginPrice { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }
    }
}
