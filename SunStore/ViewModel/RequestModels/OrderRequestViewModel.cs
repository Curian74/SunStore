namespace SunStore.ViewModel.RequestModels
{
    public class OrderRequestViewModel
    {
        public int UserId { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string? Note { get; set; } 
        public string? VoucherCode { get; set; }
        public string PaymentMethod { get; set; }
    }
}
