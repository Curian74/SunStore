using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Customer
{
    public int UserId { get; set; }

    public string Ranking { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
