using MyStore.Enumerations;
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

        //Giaohangnhanh
        public string WardID { get; set; }
        public int DistrictID { get; set; }

        public string? UserIP { get; set; }

    }

    public class UpdateOrderRequest
    {
        public string? DeliveryAddress { get; set; }
        public string? ReceiverInfo { get; set; }
    }

    public class OrderToShippingRequest
    {
        public int Length { get; set; }
        public int Weight { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public GHNRequiredNoteEnum RequiredNote { get; set; }
    }
}
