using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Image { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public int CategoryId { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;
}
