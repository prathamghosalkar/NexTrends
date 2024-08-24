using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class CouponUsage
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int CouponId { get; set; }

    public DateOnly UsageDate { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
