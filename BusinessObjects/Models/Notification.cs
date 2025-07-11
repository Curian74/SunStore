using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Content { get; set; } = null!;

    public int? OrderId { get; set; }

    public bool? IsRead { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? User { get; set; }
}
