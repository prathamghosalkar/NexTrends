using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string Password { get; set; } = null!;

    public string? Gender { get; set; }

    public string? Pincode { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
