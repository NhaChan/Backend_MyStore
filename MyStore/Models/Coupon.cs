﻿using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }
        public string Note { get; set; }
        public DateTime DateFounded { get; set; }
        public DateTime ConfirmDate { get; set; }
        public string Status { get; set; }
        public User User { get; set; }
        public Supplier Supplier { get; set; }

    }
}
