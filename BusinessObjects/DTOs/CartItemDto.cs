using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int? ProductOptionId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public string? Size { get; set; }
        public double? Price { get; set; }
        public int? Quantity { get; set; }
        public int? TotalQuantity { get; set; }
        public double? Total => Price * Quantity;
    }

}
