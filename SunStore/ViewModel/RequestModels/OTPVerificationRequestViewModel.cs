namespace SunStore.ViewModel.RequestModels
{
    public class OTPVerificationRequestViewModel
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
