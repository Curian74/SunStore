using System.ComponentModel.DataAnnotations;

namespace SunStoreAPI.Dtos
{
    public class RegisterDto
    {
        public string FullName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;

        [EmailAddress]
        public string Email { get; set; } = null!;
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        public DateOnly BirthDate { get; set; }
    }
}
