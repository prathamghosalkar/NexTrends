using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Order
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public string? Status { get; set; }

    public decimal Amount { get; set; }

    public string? ModeOfPayment { get; set; }

    public int? CouponId { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Coupon? Coupon { get; set; }
}
