using System;
using System.Collections.Generic;

namespace NexTrends.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int Cid { get; set; }

    public string? Name { get; set; }

    public string? Message { get; set; }

    public virtual Customer CidNavigation { get; set; } = null!;
}
