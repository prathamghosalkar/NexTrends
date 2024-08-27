using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
