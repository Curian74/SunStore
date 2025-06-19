using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.ApiResponses
{
    public class ProductDetailResponse
    {
        // Product info
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductImage { get; set; }
        public string? ProductDescription { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public CategoryResponseModel Category { get; set; } = null!;

        // Option selected
        public int OptionId { get; set; }
        public string? Size { get; set; }
        public int? Quantity { get; set; }
        public double? Price { get; set; }
        public double? Rating { get; set; }
        public int? Discount { get; set; }

        // List other options
        public List<ProductOptionSummaryModel> OtherOptions { get; set; } = new();
    }

    public class CategoryResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class ProductOptionSummaryModel
    {
        public int Id { get; set; }
        public string? Size { get; set; }
        public double? Price { get; set; }
    }

}
