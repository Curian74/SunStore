using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public string? Description { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    [ForeignKey("Category")]
    public int CategoryId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
}
