﻿using Microsoft.EntityFrameworkCore;

namespace MyStore.Models
{
    public class StockReceiptDetail
    {
        public long Id { get; set; }
        public int Quantity { get; set; }
        public double OriginPrice { get; set; }

        public long StockReceiptId { get; set; }
        public StockReceipt StockReceipt { get; set; }
        public int? ProductId { get; set; }
        [DeleteBehavior(DeleteBehavior.SetNull)]
        public Product Product { get; set; }
        public string ProductName { get; set; }
    }
}
