using BusinessObjects.Constants;
using Microsoft.AspNetCore.Mvc;
using SunStore.APIServices;
using System.Security.Claims;

namespace SunStore.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationAPIService _notificationAPIService;

        public NotificationController(NotificationAPIService notificationAPIService)
        {
            _notificationAPIService = notificationAPIService;
        }

        public async Task<IActionResult> Index(int? pageIndex = 1, int? pageSize = 7)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? null;

            if (role == UserRoleConstants.Admin)
            {
                userId = null;
            }

            var notifications = await _notificationAPIService.GetPaged(userId, pageIndex, pageSize);

            return View(notifications);
        }
    }
}
