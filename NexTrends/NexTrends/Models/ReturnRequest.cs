using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class ReturnRequest
{
    public int ReturnRequestId { get; set; }

    public int OrderId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalAmount { get; set; }

    public string ReturnReason { get; set; } = null!;

    public string? AdditionalDetails { get; set; }

    public DateTime RequestDate { get; set; }

    public string? Status { get; set; }

    public virtual Order Order { get; set; } = null!;
}
