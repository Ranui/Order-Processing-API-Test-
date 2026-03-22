using System;
using System.Collections.Generic;

namespace Order_Processing_API.Models;

public partial class Product
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public double UnitPrice { get; set; }

    public int AvailableStock { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
