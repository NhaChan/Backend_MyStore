﻿using Microsoft.EntityFrameworkCore;

namespace MyStore.Models
{
    [PrimaryKey(nameof(UserId), nameof(ProductId))]
    public class CartItem : IBaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
