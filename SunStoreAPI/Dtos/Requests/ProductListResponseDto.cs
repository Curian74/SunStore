namespace SunStoreAPI.Dtos.Requests
{
    public class ProductListResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }

        public CategoryResponseModel? Category { get; set; }
        public List<ProductOptionResponseModel> ProductOptions { get; set; } = new();
    }

    public class CategoryResponseModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }

    public class ProductOptionResponseModel
    {
        public int Id { get; set; }
        public string? Size { get; set; }
        public double? Price { get; set; }
    }
}
