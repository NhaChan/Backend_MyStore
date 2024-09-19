using MyStore.Models;

namespace MyStore.Request
{
    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
