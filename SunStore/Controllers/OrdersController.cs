using Microsoft.AspNetCore.Mvc;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.Constants;
using SunStore.APIServices;
using X.PagedList;
using BusinessObjects.Queries;

namespace SunStore.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderAPIService _orderService;
        private readonly UserAPIService _userAPIService;

        public OrdersController(SunStoreContext context, OrderAPIService orderAPIService, UserAPIService userAPIService)
        {
            _orderService = orderAPIService;
            _userAPIService = userAPIService;
        }

        [Authorize(Roles = UserRoleConstants.Admin)]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int? page)
        {
            int pageSize = 6;
            int currentPage = page ?? 1;

            var ordersPaged = await _orderService.GetOrdersAsync(fromDate, toDate, currentPage, pageSize);

            ViewBag.From = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.To = toDate?.ToString("yyyy-MM-dd");

            // PagedList
            var pagedList = new StaticPagedList<Order>(
                ordersPaged.Items, currentPage, pageSize, ordersPaged.TotalItems
            );

            // Statistics
            var orders = await _orderService.GetAllAsync();

            // Tổng doanh thu
            var totalRevenue = orders
                .Where(o => o.Status == OrderStatusConstant.Received)
                .Sum(o => o.TotalPrice ?? 0);

            // SL đơn hàng đã giao
            var deliveredCount = orders.Count(o => o.Status == OrderStatusConstant.Received);

            // Doanh thu hôm nay
            var today = DateTime.Now.Date;
            var todayRevenue = orders
                .Where(o => o.Status == OrderStatusConstant.Received && o.DateTime.HasValue && o.DateTime.Value.Date == today)
                .Sum(o => o.TotalPrice ?? 0);

            // Get Shippers.
            var queryObj = new UserQueryObject
            {
                CurrentPage = 1,
                PageSize = int.MaxValue,
                Role = int.Parse(UserRoleConstants.Shipper)
            };

            var shippers = await _userAPIService.GetPagedUserAsync(queryObj);

            ViewBag.Revenue = totalRevenue;
            ViewBag.NumberOrders = deliveredCount;
            ViewBag.RToday = todayRevenue;
            ViewBag.Shippers = shippers!.Data!.Items;

            return View(pagedList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var orderDetail = await _orderService.GetOrderDetailAsync(id);
            return View(orderDetail);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            var result = await _orderService.CancelOrderAsync(id, reason);
            if (result.IsSuccessful)
            {
                TempData["Success"] = "Đơn hàng đã được hủy thành công.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Không thể hủy đơn hàng.";
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<JsonResult> GetCancelReason(int id)
        {
            var reason = await _orderService.GetCancelReasonAsync(id);
            return Json(new { reason });
        }
    }
}
