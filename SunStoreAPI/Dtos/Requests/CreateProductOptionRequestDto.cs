using System.ComponentModel.DataAnnotations;

namespace SunStoreAPI.Dtos.Requests
{
    public class CreateProductOptionRequestDto
    {
        public string Size { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 0.")]
        public double? Price { get; set; }

        public int ProductId { get; set; }
    }
}
