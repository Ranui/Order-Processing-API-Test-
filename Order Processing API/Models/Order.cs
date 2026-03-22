using System;
using System.Collections.Generic;

namespace Order_Processing_API.Models;

public partial class Order
{
    public int Id { get; set; }

    public string CustomerEmail { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string CreatedAtUtc { get; set; } = null!;

    public string? ConfirmedAtUtc { get; set; }

    public string? CancelledAtUtc { get; set; }

    public double TotalAmount { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
