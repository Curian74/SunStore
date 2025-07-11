namespace SunStoreAPI.Services
{
    public interface INotificationService
    {
        Task NotifyAdminsNewOrder(string message);
        Task NotifyShipper(string message, int shipperId);
        Task NotifyCustomer(string message, int customerId);
    }
}
