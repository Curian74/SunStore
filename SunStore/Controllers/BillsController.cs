using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using SunStore.APIServices;
using System.Security.Claims;
using X.PagedList;

namespace SunStore.Controllers
{
    public class BillsController : Controller
    {
        private readonly BillAPIService _billService;

        public BillsController(BillAPIService billService)
        {
            _billService = billService;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int? page)
        {
            var result = await _billService.GetBillsAsync(fromDate, toDate, page ?? 1);

            ViewBag.From = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.To = toDate?.ToString("yyyy-MM-dd");

            var pagedList = new StaticPagedList<Order>(
                result.Items,
                result.CurrentPage,
                result.PageSize,
                result.TotalItems
            );

            return View(pagedList);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var bill = await _billService.GetBillDetailAsync(id);
            if (bill == null)
                return NotFound();
            return View(bill);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var response = await _billService.CancelBillAsync(id);
            if (response.IsSuccessful)
                return RedirectToAction("Index");
            else
                return BadRequest("Không thể huỷ đơn hàng.");
        }

        [HttpGet]
        public async Task<JsonResult> GetCancelReason(int id)
        {
            var reason = await _billService.GetCancelReasonAsync(id);
            return Json(new { reason });
        }
    }
}
