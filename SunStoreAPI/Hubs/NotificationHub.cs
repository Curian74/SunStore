using BusinessObjects.Constants;
using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SunStoreAPI.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userRole == UserRoleConstants.Admin)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
