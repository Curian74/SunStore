using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using SunStore.ViewModel.RequestModels;
using SunStore.APIServices;
using System.Security.Claims;
using System.Text.Json;

namespace SunStore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CheckoutAPIService _checkoutService;

        public CheckoutController(CheckoutAPIService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return RedirectToAction("Login", "Account");

            var userId = int.Parse(userIdStr);
            var checkoutInfo = await _checkoutService.GetCheckoutInitInfoAsync(userId);

            if (checkoutInfo == null)
                return RedirectToAction("Cart", "Cart");

            return View(checkoutInfo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBill(string address, string number, string note, string voucher, string payment)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdStr);

            var model = new OrderRequestViewModel
            {
                UserId = userId,
                Address = address,
                PhoneNumber = number,
                Note = note,
                VoucherCode = voucher,
                PaymentMethod = payment,
            };
            var result = await _checkoutService.CreateOrderAsync(model);

            if (result.IsSuccessful && result.Data.ValueKind != JsonValueKind.Undefined && result.Data.ValueKind != JsonValueKind.Null)
            {
                var root = result.Data;

                if (root.TryGetProperty("vnpUrl", out var vnpUrl))
                {
                    return Redirect(vnpUrl.GetString());
                }

                return RedirectToAction("Index", "Home");
            }

            TempData["Message"] = "Tạo đơn hàng thất bại.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit(string address, string number, string note, string voucher, string payment)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdStr);

            var model = new OrderRequestViewModel
            {
                UserId = userId,
                Address = address,
                PhoneNumber = number,
                Note = note,
                VoucherCode = voucher,
                PaymentMethod = payment
            };

            var result = await _checkoutService.CreateDepositVNPayAsync(model);

            if (result.IsSuccessful && result.Data.TryGetProperty("vnpUrl", out var url))
            {
                return Json(new { vnpUrl = url.GetString() });
            }

            return BadRequest("Không tạo được thanh toán cọc.");
        }



        [HttpGet]
        public async Task<JsonResult> UseVoucher(string code)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Json(new { exist = false });

            int uid = int.Parse(userIdStr);
            var result = await _checkoutService.UseVoucherAsync(code, uid);
            return Json(result);
        }

        public IActionResult PaymentFail() => View();

        public IActionResult PaymentCallBack(bool success, int? orderId, string? message)
        {
            if (success)
            {
                TempData["Message"] = "Thanh toán VN Pay thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Message"] = message ?? "Thanh toán thất bại.";
                return RedirectToAction("Index");
            }
        }

    }
}
