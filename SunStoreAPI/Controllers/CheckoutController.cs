using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunStoreAPI.Dtos;
using SunStoreAPI.Dtos.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.WebUtilities;
using SunStoreAPI.Services;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly SunStoreContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly EmailService _emailService;
        private readonly INotificationService _notificationService;

        public CheckoutController(SunStoreContext context, IVnPayService vnPayService,
            EmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _vnPayService = vnPayService;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        [HttpGet("init-checkout/{userId}")]
        public async Task<IActionResult> GetCheckoutInfo(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var items = await _context.OrderItems
                .Include(x => x.ProductOption)
                .ThenInclude(p => p.Product)
                .Where(x => x.CustomerId == userId && x.OrderId == 0)
                .ToListAsync();

            if (user == null || items == null) return NotFound();

            var response = new
            {
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Address,
                Items = items.Select(x => new
                {
                    x.Id,
                    ProductOptionId = x.ProductId,
                    x.ProductOption.Product.Name,
                    x.ProductOption.Size,
                    x.ProductOption.Price,
                    x.Quantity,
                    SubTotal = x.Price
                }),
                Total = items.Sum(x => x.Price)
            };

            return Ok(response);
        }

        [HttpPost("CreateBill")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return BadRequest("User not found");

            var cartItems = await _context.OrderItems
                .Where(o => o.CustomerId == request.UserId && o.OrderId == 0)
                .ToListAsync();

            if (!cartItems.Any()) return BadRequest("Cart is empty");

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == request.VoucherCode);
            int? voucherId = voucher?.VoucherId;

            double? total = cartItems.Sum(x => x.Price) + 20000;
            double? finalTotal = total;

            if (voucher != null)
            {
                finalTotal -= (total * voucher.Vpercent / 100);
            }

            if (request.PaymentMethod == "Thanh toán VNPAY")
            {

                var returnUrl = QueryHelpers.AddQueryString("https://localhost:7270/api/Checkout/PaymentCallBack", new Dictionary<string, string?>
                {
                    ["address"] = request.Address,
                    ["phone"] = request.PhoneNumber,
                    ["note"] = request.Note ?? "",
                    ["voucherId"] = voucherId?.ToString() ?? "",
                    ["userId"] = request.UserId.ToString()
                });

                var vnPayUrl = _vnPayService.CreatePaymentUrl(HttpContext, new VnPaymentRequestModel
                {
                    Amount = finalTotal ?? 0,
                    CreatedDate = DateTime.Now,
                    Description = $"{request.Address}_{request.PhoneNumber}",
                    FullName = user.FullName,
                    OrderId = new Random().Next(100000, 999999),
                    ReturnUrl = returnUrl
                });

                return Ok(new { VnpUrl = vnPayUrl });
            }

            var order = new Order
            {
                DateTime = DateTime.Now,
                AdrDelivery = request.Address.Trim(),
                PhoneNumber = request.PhoneNumber,
                VoucherId = voucherId,
                Status = "Đã đặt hàng",
                Payment = "COD",
                Note = request.Note,
                TotalPrice = finalTotal,
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var product = await _context.ProductOptions.FindAsync(item.ProductId);
                product.Quantity -= item.Quantity;
                item.OrderId = order.Id;
                _context.ProductOptions.Update(product);
                _context.OrderItems.Update(item);
            }

            if (voucher != null && (voucher.Quantity == null || voucher.Quantity > 0))
            {
                order.TotalPrice = finalTotal;
                if(voucher.Quantity != null)
                {
                    voucher.Quantity--;
                }
                _context.Vouchers.Update(voucher);
            }

            await _context.SaveChangesAsync();

            string mailContent = $"<html><body> Quý khách đã đặt hàng thành công. Đơn hàng sẽ được vận chuyển sớm nhất có thể." +
                                 $"<br> Thời gian: {DateTime.Now}" +
                                 $"<br><br>Sun Store trân trọng cảm ơn quý khách! </body></html>";

            await _emailService.SendEmailAsync(user.Email!, "[Sun Store] Đặt hàng thành công", mailContent);

            var notiContent = $"Đơn hàng mới từ {user.Username}";

            var notification = new Notification
            {
                Content = notiContent,
                CreatedAt = DateTime.Now,
                CreatedBy = user.Id,
                IsDeleted = false,
                IsRead = false,
                OrderId = order.Id,
                UserId = null,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyAdminsNewOrder(notiContent);

            return Ok(new { Message = "Đặt hàng thành công", OrderId = order.Id });
        }

        [HttpGet("PaymentCallBack")]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExcute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                return Redirect("https://localhost:7127/Checkout/PaymentCallBack?success=false&message=Thanh toán thất bại");
            }

            string address = Request.Query["address"];
            string phone = Request.Query["phone"];
            string note = Request.Query["note"];
            string vch = Request.Query["voucherId"];
            int? voucherId = null;
            if (!string.IsNullOrEmpty(vch))
            {
                voucherId = int.Parse(vch);
            }
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
            int? userId = uid == null ? null : int.Parse(uid);

            var order = new Order
            {
                DateTime = DateTime.Now,
                AdrDelivery = address,
                PhoneNumber = phone,
                VoucherId = voucherId,
                Status = "Đã đặt hàng",
                Payment = "VNPAY",
                Note = note,
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var cartItems = await _context.OrderItems
                .Where(x => x.CustomerId == userId && x.OrderId == 0)
                .ToListAsync();

            double? total = 0;
            foreach (var item in cartItems)
            {
                var product = await _context.ProductOptions.FindAsync(item.ProductId);
                product.Quantity -= item.Quantity;
                item.OrderId = order.Id;
                total += item.Price;
                _context.OrderItems.Update(item);
                _context.ProductOptions.Update(product);
            }

            total += 20000;
            if (voucherId != null)
            {
                var voucher = await _context.Vouchers.FindAsync(voucherId);
                total -= (total * voucher.Vpercent / 100);
                if (voucher.Quantity != null)
                {
                    voucher.Quantity--;
                }
                _context.Vouchers.Update(voucher);
            }

            order.TotalPrice = total;
            _context.Orders.Update(order);

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            string mailContent = $"<html><body> Quý khách đã đặt hàng thành công. Đơn hàng sẽ được vận chuyển sớm nhất có thể." +
                                  $"<br> Thời gian: {DateTime.Now}" +
                                  $"<br><br>Sun Store trân trọng cảm ơn quý khách! </body></html>";

            await _emailService.SendEmailAsync(user.Email!, "[Sun Store] Đặt hàng thành công", mailContent);

            var notiContent = $"Đơn hàng mới từ {user.Username}";

            var notification = new Notification
            {
                Content = notiContent,
                CreatedAt = DateTime.Now,
                CreatedBy = user.Id,
                IsDeleted = false,
                IsRead = false,
                OrderId = order.Id,
                UserId = null,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyAdminsNewOrder(notiContent);

            return Redirect($"https://localhost:7127/Checkout/PaymentCallBack?success=true&orderId={order.Id}");
        }

        [HttpGet("use-voucher")]
        public async Task<IActionResult> UseVoucher([FromQuery] string code, [FromQuery] int userId)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code);

            bool inuse = false;
            bool exist = false;
            bool remain = false;
            int percent = 0;

            if (voucher != null)
            {
                exist = true;
                percent = voucher.Vpercent;

                var now = DateTime.Now;
                if ((voucher.Quantity == null || voucher.Quantity > 0) && voucher.StartDate <= now && voucher.EndDate >= now)
                {
                    remain = true;
                }

                var ordersWithVoucher = await _context.Orders
                    .Where(o => o.VoucherId == voucher.VoucherId)
                    .ToListAsync();

                foreach (var order in ordersWithVoucher)
                {
                    var used = await _context.OrderItems
                        .AnyAsync(i => i.OrderId == order.Id && i.CustomerId == userId);

                    if (used)
                    {
                        inuse = true;
                        break;
                    }
                }

                if (voucher.VoucherCustomers != null && !voucher.VoucherCustomers.Any(v => v.CustomerId == userId))
                {
                    inuse = true;
                }
            }

            return Ok(new
            {
                exist,
                remain,
                inuse,
                percent
            });
        }
    }
}
