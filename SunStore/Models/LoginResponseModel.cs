namespace SunStore.Models
{
    public class LoginResponseModel
    {
        public string? Token { get; set; }
        public bool? IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
