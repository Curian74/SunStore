using System.ComponentModel.DataAnnotations;

namespace SunStore.ViewModel
{
    public class LoginRequestViewModel
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
