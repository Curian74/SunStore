using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel.RequestModels
{
    public class EditProductRequestViewModel
    {
        public List<SelectListItem>? Categories { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int? CategoryId { get; set; }

        [DisplayName("Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string Name { get; set; } = null!;

        [DisplayName("Hình ảnh")]
        public string? ImageUrl { get; set; }

        [DisplayName("Mô tả")]
        public string? Description { get; set; }

        [DisplayName("Còn bán?")]
        public bool? IsDeleted { get; set; }
    }
}
