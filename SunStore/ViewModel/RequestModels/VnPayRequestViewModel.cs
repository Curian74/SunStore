namespace SunStore.ViewModel.RequestModels
{
    public class VnPayRequestViewModel
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
    }
}
