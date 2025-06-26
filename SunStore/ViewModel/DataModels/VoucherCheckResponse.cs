namespace SunStore.ViewModel.DataModels
{
    public class VoucherCheckResponse
    {
        public bool exist { get; set; }
        public bool remain { get; set; }
        public bool inuse { get; set; }
        public int percent { get; set; }
    }
}
