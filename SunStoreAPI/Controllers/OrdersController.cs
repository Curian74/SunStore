using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.Constants;
using BusinessObjects.ApiResponses;
using BusinessObjects;
using SunStoreAPI.Services;
using System.Threading.Tasks;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly SunStoreContext _context;
        private readonly INotificationService _notificationService;

        public OrdersController(SunStoreContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetOrders(DateTime? fromDate, DateTime? toDate, int? page = 1, int pageSize = 6)
        {
            var orders = await _context.Orders.ToListAsync();

            if (fromDate.HasValue)
                orders = orders.Where(o => o.DateTime >= fromDate.Value).ToList();

            if (toDate.HasValue)
                orders = orders.Where(o => o.DateTime <= toDate.Value.AddDays(1)).ToList();

            var result = new PagedResult<Order>
            {
                CurrentPage = page ?? 1,
                PageSize = pageSize,
                TotalItems = orders.Count-1,
                Items = orders.OrderByDescending(o => o.DateTime)
                                .Skip(((page ?? 1) - 1) * pageSize)
                                .Take(pageSize)
                                .ToList()
            };

            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var items = await _context.OrderItems.Include(o => o.ProductOption)
                                                    .ThenInclude(p => p.Product)
                                                 .Where(o => o.OrderId == id)
                                                 .ToListAsync();

            var user = await _context.Users.FindAsync(items.FirstOrDefault()?.CustomerId);
            var shipper = await _context.Users.FindAsync(order.ShipperId);
            var voucher = await _context.Vouchers.FindAsync(order.VoucherId);

            var detail = new BillDetailDto
            {
                Items = items.Select(x => new OrderItemDto
                {
                    Id = x.Id,
                    OrderId = order.Id,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    ProductOptionSize = x.ProductOption?.Size,
                    ProductOptionName = x.ProductOption?.Product.Name
                }).ToList(),
                Order = new OrderDto
                {
                    Id = order.Id,
                    DateTime = order.DateTime,
                    TotalPrice = order.TotalPrice,
                },
                CustomerName = user?.FullName,
                Phone = order.PhoneNumber,
                Address = order.AdrDelivery,
                Status = order.Status,
                Note = order.Note,
                Payment = order.Payment,
                VoucherPercent = voucher?.Vpercent ?? 0,
                Shipper = shipper?.FullName ?? "Chưa có",
                VoucherCode = voucher?.Code ?? "Không có"
            };

            return Ok(detail);
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromQuery] string reason = "default")
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var items = await _context.OrderItems.Where(x => x.OrderId == id).ToListAsync();
            foreach (var item in items)
            {
                var product = await _context.ProductOptions.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    _context.ProductOptions.Update(product);
                }
            }

            order.Status = "Bị từ chối";
            order.DenyReason = reason == "default" ? "Xin lỗi quý khách, hiện tại shop không thể ship hàng. Mong quý khách thông cảm." : reason;
            _context.Orders.Update(order);

            await _context.SaveChangesAsync();
            return Ok(new BaseApiResponse
            {
                IsSuccessful = true,
                Message = "Order cancelled successfully."
            });
        }

        [HttpGet("cancel-reason/{id}")]
        public async Task<IActionResult> GetCancelReason(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            var reason = order?.DenyReason ?? "Xin lỗi quý khách, hiện tại shop không thể ship hàng. Mong quý khách thông cảm.";
            return Ok(new { reason });
        }

        [HttpGet("assigning")]
        public async Task<IActionResult> GetOrderForAssigning(int orderId, int shipperId)
        {
            var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ShipperId == shipperId);

            if (order == null)
            {
                return NotFound(new ApiResult<Order>
                {
                    IsSuccessful = false,
                });
            }

            return Ok(new ApiResult<Order>
            {
                Data = order,
                IsSuccessful = true,
            });
        }

        [HttpPost("assign")]
        public async Task<IActionResult> Assign(int orderId, int shipperId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ShipperId == null);

            if (order == null)
            {
                return NotFound();
            }

            // Update order info.
            order.ShipperId = shipperId;

            // Notify the targeted shipper.
            var notificationContentForShipper = $"Bạn đã được chỉ định giao hàng cho đơn hàng có mã: {order.Id}.";

            await _notificationService.NotifyShipper(notificationContentForShipper, shipperId);

            var notification = new Notification
            {
                Content = notificationContentForShipper,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                IsRead = false,
                OrderId = order.Id,
                UserId = shipperId,
            };

            await _context.Notifications.AddAsync(notification);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("shipper-pending/{shipperId}")]
        public async Task<IActionResult> GetShipperPendingOrder(int shipperId,
            int? page = 1, int? pageSize = 5)
        {
            var orders = await _context.Orders
                .Where(x => x.ShipperId == shipperId)
                .OrderByDescending(x => x.DateTime)
                .ToListAsync();

            var skip = (page - 1) * pageSize;

            var totalOrder = orders.Count;

            var pagedOrders = orders.Skip((int)skip!).Take((int) pageSize!).ToList();

            var returnData = new PagedResult<Order>
            {
                Items = pagedOrders,
                CurrentPage = (int) page!,
                PageSize = (int) pageSize, 
                TotalItems = totalOrder,
            };

            return Ok(returnData);
        }

        [HttpGet]
        [Route("unassigned")]
        [Authorize(Roles = UserRoleConstants.Admin)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersForShipperAssignment()
        {
            return await _context.Orders
                .Where(o => o.ShipperId == null && o.Status == "Đã đặt hàng")
                .ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
