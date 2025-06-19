namespace SunStoreAPI.Dtos
{
    public class VerifyPasswordOtpDto
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
