using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string? Status { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
