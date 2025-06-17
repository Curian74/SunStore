namespace SunStoreAPI.Dtos
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public bool? IsSuccessful { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
}
