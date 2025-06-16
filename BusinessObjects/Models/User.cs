using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Address { get; set; }

    public DateOnly BirthDate { get; set; }

    public string? Email { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public int Role { get; set; }

    public string? VertificationCode { get; set; }

    public int? IsBanned { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? Employee { get; set; }
}
