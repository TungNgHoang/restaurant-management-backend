using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Dtos;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserAccountService _userAccountService;
        public AuthController(IUserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                // Gọi service để đăng nhập và tạo token
                var token = await _userAccountService.LoginAsync(loginRequest);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Nếu có lỗi (user không tồn tại hoặc mật khẩu không khớp), trả về 401 Unauthorized
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Bắt các exception khác (nếu cần)
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
