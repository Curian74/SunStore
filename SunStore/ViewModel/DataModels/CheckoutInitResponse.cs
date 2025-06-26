namespace SunStore.ViewModel.DataModels
{
    public class CheckoutInitResponse
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public List<CheckoutItemDto> Items { get; set; } = new();
        public double? Total { get; set; }
    }

    public class CheckoutItemDto
    {
        public int Id { get; set; }
        public int ProductOptionId { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double SubTotal { get; set; }
    }
}
