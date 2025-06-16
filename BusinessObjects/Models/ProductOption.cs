using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ProductOption
{
    public int Id { get; set; }

    public string? Size { get; set; }

    public int? Quantity { get; set; }

    public double? Price { get; set; }

    public double? Rating { get; set; }

    public int? Discount { get; set; }

    public int ProductId { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;
}
