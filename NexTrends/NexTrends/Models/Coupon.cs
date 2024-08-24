using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Coupon
{
    public int Id { get; set; }

    public string CouponCode { get; set; } = null!;

    public decimal DiscountPercentage { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public string? Occasion { get; set; }

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
