using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class VoucherCustomer
{
    public int Id { get; set; }

    public int? VoucherId { get; set; }

    public int? CustomerId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Voucher? Voucher { get; set; }
}
