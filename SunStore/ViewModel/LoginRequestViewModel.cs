using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel
{
    public class LoginRequestViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }
    }
}
