using System;
using System.Collections.Generic;

namespace Order_Processing_API.Models;

public partial class OrderItem
{
    public int OrderId { get; set; }

    public string ProductId { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public double UnitPrice { get; set; }

    public int Quantity { get; set; }

    public double LineTotal { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
