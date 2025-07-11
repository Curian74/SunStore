using BusinessObjects;
using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly SunStoreContext _context;

        public NotificationController(SunStoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedByUserId(string? userId, int? pageIndex = 1, int? pageSize = 7)
        {
            var notifications = _context.Notifications
                .Where(n => n.IsDeleted == false)
                .OrderByDescending(n => n.CreatedAt)
                .AsQueryable();

            if (userId != null)
            {
                notifications = notifications.Where(x => x.UserId.ToString() == userId);
            }

            var skip = (pageIndex - 1) * pageSize;

            var data = await notifications.Skip(skip!.Value).Take(pageSize!.Value).ToListAsync();

            var pagedData = new PagedResult<Notification>
            {
                Items = data,
                PageSize = pageSize.Value,
                TotalItems = notifications.Count(),
                CurrentPage = pageIndex!.Value,
            };

            return Ok(pagedData);
        }

        [HttpPut("{notificationId}")]
        public async Task<IActionResult> Deactivate(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new BaseApiResponse
            {
                IsSuccessful = true,
            });
        }
    }
}
