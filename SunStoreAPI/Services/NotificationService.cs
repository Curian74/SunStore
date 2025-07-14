
using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using SunStoreAPI.Hubs;

namespace SunStoreAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAdminsNewOrder(string message)
        {
            await _hubContext.Clients.Group("admin").SendAsync("ReceiveNotification", message);
        }

        public async Task NotifyCustomer(string message, int customerId)
        {
            await _hubContext.Clients.Group($"user_{customerId}").SendAsync("ReceiveNotification", message);
        }

        public async Task NotifyShipper(string message, int shipperId)
        {
            await _hubContext.Clients.Group($"user_{shipperId}").SendAsync("ReceiveNotification", message);
        }
    }
}
