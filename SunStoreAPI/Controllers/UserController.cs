using BusinessObjects;
using BusinessObjects.ApiResponses;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunStoreAPI.Dtos.Requests;
using SunStoreAPI.Dtos.User;
using BusinessObjects.Queries;

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

        [HttpGet("filter")]
        public async Task<IActionResult> GetPaged([FromQuery] UserQueryObject userQueryObject)
        {
            var users = _context.Users.AsQueryable();

            var skip = (userQueryObject.CurrentPage - 1) * userQueryObject.PageSize;

            var filteredData = await users.Skip(skip).Take(userQueryObject.PageSize).ToListAsync();

            var pagedData = new PagedResult<UserDto>
            {
                PageSize = userQueryObject.PageSize,
                CurrentPage = userQueryObject.CurrentPage,
                TotalItems = users.Count(),
                Items = filteredData.Select(u => new UserDto
                {
                    Id = u.Id,
                    Address = u.Address,
                    BirthDate = u.BirthDate,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsBanned = u.IsBanned,
                    Password = u.Password,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    Username = u.Username,
                })
                .ToList(),
            };

            return Ok(new ApiResult<PagedResult<UserDto>>
            {
                Data = pagedData,
                IsSuccessful = true,
            });
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
