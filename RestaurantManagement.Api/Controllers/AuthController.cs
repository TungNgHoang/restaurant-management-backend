using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Dtos;
using RestaurantManagement.Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IUserAccountService _userAccountService;
        public AuthController(IServiceProvider serviceProvider, IUserAccountService userAccountService) : base(serviceProvider)
        {    
            _userAccountService = userAccountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            // Gọi service để đăng nhập và tạo token
            var token = await _userAccountService.LoginAsync(loginRequest);
            // Sử dụng JwtSecurityTokenHandler để giải mã token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            // Lấy claim Role (có thể là ClaimTypes.Role hoặc "role" tùy vào cách bạn đặt)
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Trả về token và role trong response
            return Success(new { token, role = roleClaim });
            
        }
    }
}
