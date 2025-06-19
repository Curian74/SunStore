using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel
{
    public class UpdatePasswordRequestViewModel
    {
        public string Email { get; set; } = null!;
        [MinLength(6, ErrorMessage = "Password phải dài ít nhất 6 kí tự.")]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = null!;

        public string Otp { get; set; } = null!;
    }
}
