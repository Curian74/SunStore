using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? CustomerId { get; set; }

    public int? OrderId { get; set; }

    public int? Quantity { get; set; }

    public double? Price { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ProductOption? ProductOption { get; set; }
}
