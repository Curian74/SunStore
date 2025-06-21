using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunStoreAPI.Dtos.Requests;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SunStoreContext _context;

        public UserController(SunStoreContext context)
        {
            _context = context;
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(UpdateUserRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (user == null)
            {
                return NotFound(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Không tìm thấy người dùng."
                });
            }

            var emailExisted = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == dto.Email && x.Id != dto.Id);

            var phoneExisted = await _context.Users
                    .FirstOrDefaultAsync(x => x.PhoneNumber == dto.PhoneNumber && x.Id != dto.Id);

            if (emailExisted != null)
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Email này đã tồn tại."
                });
            }

            if (phoneExisted != null)
            {
                return BadRequest(new BaseApiResponse
                {
                    IsSuccessful = false,
                    Message = "Số điện thoại này đã tồn tại."
                });
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address = dto.Address;
            user.Email = dto.Email;
            user.BirthDate = dto.BirthDate;
            user.IsBanned = dto.IsBanned != null ? dto.IsBanned : user.IsBanned;
            user.Role = (int)(dto.Role != null ? dto.Role : user.Role);
            user.Username = dto.Username != null ? dto.Username : user.Username;

            await _context.SaveChangesAsync();

            return Ok(new BaseApiResponse
            {
                IsSuccessful = true,
                Message = "Cập nhật thông tin thành công."
            });
        }
    }
}
