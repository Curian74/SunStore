using System.ComponentModel.DataAnnotations;

namespace SunStoreAPI.Dtos
{
    public class RegisterDto
    {
        public string FullName { get; set; } = null!;

        [MinLength(6, ErrorMessage = "Password phải dài ít nhất 6 kí tự.")]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = null!;

        [EmailAddress]
        public string Email { get; set; } = null!;
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        public DateOnly BirthDate { get; set; }
    }
}
