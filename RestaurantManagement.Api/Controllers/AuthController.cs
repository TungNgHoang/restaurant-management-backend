﻿namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IUserAccountService _userAccountService;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IServiceProvider serviceProvider, IUserAccountService userAccountService, IAuthService authService, ILogger<AuthController> logger) : base(serviceProvider)
        {    
            _userAccountService = userAccountService;
            _authService = authService;
            _logger = logger;
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

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            _logger.LogInformation("Auth header: " + authHeader);
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new { message = "Token not provided." });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            _logger.LogInformation("Extracted token: " + token);
            await _authService.LogoutAsync(token);
            return Ok(new { message = "Logout successful. Token has been revoked." });
        }
    }
}
