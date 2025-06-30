using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.Constants;
using BusinessObjects.ApiResponses;
using System.Security.Claims;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly SunStoreContext _context;

        public OrdersController(SunStoreContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
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
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.ShipperId == null);

            if (order == null)
            {
                return NotFound();
            }

            order.ShipperId = shipperId;

            await _context.SaveChangesAsync();

            return NoContent();
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
