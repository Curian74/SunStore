using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string? Code { get; set; }

    public int Vpercent { get; set; }

    public int Quantity { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
