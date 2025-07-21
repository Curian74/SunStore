using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string? Code { get; set; }

    public int Vpercent { get; set; }
    [Range(0, int.MaxValue)]
    public int? Quantity { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<VoucherCustomer>? VoucherCustomers { get; set; } = null;
}
