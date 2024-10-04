using System.ComponentModel.DataAnnotations;

namespace MyStore.Request
{
    public class OrderRequest
    {
        public double Total { get; set; }
        public double ShippingCost { get; set; }

        [MaxLength(100, ErrorMessage = "Thông tin quá dài")]
        public string Receiver { get; set; }

        [MaxLength(100, ErrorMessage = "Địa chỉ quá dài")]
        public string DeliveryAddress { get; set; }
        public string? Code { get; set; }
        public IEnumerable<int> CartIds { get; set; }
        public int PaymentMethodId { get; set; }
        public string? UserIP { get; set; }

    }
}
