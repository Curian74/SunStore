
using System.ComponentModel.DataAnnotations;

namespace SunStoreAPI.Dtos.Requests
{
    public class CreateUserRequestDto
    {
        public string FullName { get; set; } = null!;

        public string Username { get; set; } = null!;

        [MinLength(6, ErrorMessage = "Password phải dài ít nhất 6 kí tự.")]
        public string Password { get; set; } = null!;
        
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = null!;

        public string? Address { get; set; }

        public DateOnly BirthDate { get; set; }

        [EmailAddress]
        public string Email { get; set; } = null!;

        [MaxLength(10)]
        public string PhoneNumber { get; set; } = null!;

        public int? Role { get; set; }

        public int? IsBanned { get; set; }

    }
}