using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SunStoreAPI.Dtos.Requests
{
    public class CreateProductRequestDto
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string Name { get; set; } = null!;

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
