using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using System.Security.Claims;
using BusinessObjects;
using BusinessObjects.ApiResponses;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillsController : ControllerBase
    {
        private readonly SunStoreContext _context;

        public BillsController(SunStoreContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetBills(
            DateTime? fromDate,
            DateTime? toDate,
            int? page = 1,
            int pageSize = 6)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            int uid = int.Parse(userId);

            var query = _context.Orders
                .Where(o => _context.OrderItems.Any(oi => oi.CustomerId == uid && oi.OrderId == o.Id));

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.DateTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.DateTime <= toDate.Value.AddDays(1));
            }

            var totalItems = await query.CountAsync();

            var pagedOrders = await query
                .OrderByDescending(o => o.DateTime)
                .Skip((page.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Order>
            {
                CurrentPage = page.Value,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = pagedOrders
            };

            return Ok(result);
        }


        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var items = await _context.OrderItems
                .Include(o => o.ProductOption)
                    .ThenInclude(p => p.Product)
                .Where(o => o.OrderId == id)
                .ToListAsync();

            var uid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FindAsync(uid);

            var voucher = order.VoucherId != null ? await _context.Vouchers.FindAsync(order.VoucherId) : null;
            var shipper = order.ShipperId != null ? await _context.Users.FindAsync(order.ShipperId) : null;

            var itemDtos = items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                ProductOptionName = i.ProductOption?.Product.Name!,
                ProductOptionSize = i.ProductOption?.Size!,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                DateTime = order.DateTime,
                TotalPrice = order.TotalPrice,
            };

            return Ok(new BillDetailDto
            {
                Items = itemDtos,
                Order = orderDto,
                CustomerName = user?.FullName,
                Phone = order.PhoneNumber,
                Address = order.AdrDelivery!,
                Status = order.Status!,
                Note = order.Note,
                Payment = order.Payment!,
                VoucherPercent = voucher?.Vpercent ?? 0,
                Shipper = shipper?.FullName ?? "Chưa có",
                VoucherCode = voucher?.Code ?? "Không có"
            });
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var items = await _context.OrderItems.Where(o => o.OrderId == id).ToListAsync();

            foreach (var item in items)
            {
                var product = await _context.ProductOptions.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    _context.ProductOptions.Update(product);
                }
            }

            order.Status = "Đã huỷ";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new BaseApiResponse
            { 
                IsSuccessful = true,
                Message = "Đã huỷ đơn hàng." 
            });
        }

        [HttpGet("cancel-reason/{id}")]
        public async Task<IActionResult> GetCancelReason(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var reason = string.IsNullOrEmpty(order.DenyReason)
                ? "Xin lỗi quý khách, hiện tại shop không thể ship hàng. Mong quý khách thông cảm."
                : order.DenyReason;

            return Ok(new { Reason = reason });
        }
    }
}
