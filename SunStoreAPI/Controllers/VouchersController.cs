using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunStoreAPI.Dtos.Requests;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly SunStoreContext _context;

        public VouchersController(SunStoreContext context)
        {
            _context = context;
        }

        // GET: api/Vouchers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetVouchers()
        {
            return await _context.Vouchers.ToListAsync();
        }

        // GET: api/Vouchers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Voucher>> GetVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);

            if (voucher == null)
            {
                return NotFound();
            }

            return voucher;
        }

        // POST: api/Vouchers
        [HttpPost]
        public async Task<ActionResult<Voucher>> CreateVoucher(CreateVoucherRequestDto dto)
        {
            if (_context.Vouchers.Any(v => v.Code == dto.Code))
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Voucher đã tồn tại."
                });
            }

            var voucher = new Voucher
            {
                Code = dto.Code,
                EndDate = dto.EndDate,
                Quantity = dto.Quantity,
                StartDate = dto.StartDate,
                Vpercent = dto.Vpercent,
            };

            await _context.Vouchers.AddAsync(voucher);
            await _context.SaveChangesAsync();

            if (dto.UserIds != null)
            {
                foreach (var v in dto.UserIds)
                {
                    var customerVoucher = new VoucherCustomer
                    {
                        VoucherId = voucher.VoucherId,
                        CustomerId = v
                    };
                    await _context.VoucherCustomers.AddAsync(customerVoucher);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new BaseApiResponse
            {
                IsSuccessful = true,
                Message = "Thành công."
            });
        }

        // PUT: api/Vouchers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVoucher(int id, Voucher voucher)
        {
            if (id != voucher.VoucherId)
            {
                return BadRequest();
            }

            _context.Entry(voucher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VoucherExists(id))
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

        // DELETE: api/Vouchers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.VoucherId == id);
        }
    }
}
