using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel.RequestModels
{
    public class CreateProductOptionRequestViewModel
    {
        public List<SelectListItem>? Products { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn 1 sản phẩm.")]
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập size.")]
        public string Size { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [DisplayName("Số lượng")]
        public int Quantity { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 0.")]
        [Required(ErrorMessage = "Vui lòng nhập giá tiền.")]
        [DisplayName("Giá tiền (VND)")]
        public double? Price { get; set; }

    }
}
