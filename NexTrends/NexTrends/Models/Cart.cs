using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
