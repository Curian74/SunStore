using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunStoreAPI.Dtos;
using SunStoreAPI.Utils;

namespace SunStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SunStoreContext _context;
        private readonly JwtTokenProvider _jwtTokenProvider;

        private const string JWT_COOKIE_NAME = "jwtToken";

        public AuthController(SunStoreContext context, JwtTokenProvider jwtTokenProvider)
        {
            _context = context;
            _jwtTokenProvider = jwtTokenProvider;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var loginResponseDto = new LoginResponseDto();

            // 1. Check if user exists.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                loginResponseDto.ErrorMessage = "Không tìm thấy tài khoản.";
                return BadRequest(loginResponseDto);
            }

            // 2. Check if user is banned.
            if (user.IsBanned == 1)
            {
                loginResponseDto.ErrorMessage = "Tài khoản của bạn đã bị chặn!";
                return Unauthorized(loginResponseDto);
            }

            // 3. Check password.
            if (dto.Password != user.Password)
            {
                loginResponseDto.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return Unauthorized(loginResponseDto);
            }

            //if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            //{
            //    loginResponseDto.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            //    return Unauthorized(loginResponseDto);
            //}

            // 4. Create JWT.
            string jwtToken = _jwtTokenProvider.CreateToken(user);

            // 5. Set JWT in cookie.
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            Response.Cookies.Append(JWT_COOKIE_NAME, jwtToken, cookieOptions);

            loginResponseDto.IsSuccessful = true;
            loginResponseDto.Token = jwtToken;

            return Ok(loginResponseDto);
        }

        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(JWT_COOKIE_NAME);

            return Ok("Logged out");
        }
    }
}
