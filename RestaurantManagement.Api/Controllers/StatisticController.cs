using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Interfaces;
using System.Security.Claims;

namespace RestaurantManagement.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : BaseApiController
    {
        private readonly IStatisticService _statisticService;

        public StatisticController(IServiceProvider serviceProvider, IStatisticService statisticService) : base(serviceProvider)
        {
            _statisticService = statisticService ?? throw new ArgumentNullException(nameof(statisticService));
        }

        [Authorize(Policy = "AdminManagerPolicy")] // Giới hạn cho admin, hoặc điều chỉnh theo nhu cầu
        [HttpPost("get-statistic")]
        public async Task<IActionResult> GetStatistics([FromBody] StatisticsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Dữ liệu đầu vào không hợp lệ" });

            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                // Gửi userEmail và userRole để lọc dữ liệu nếu cần
                var result = await _statisticService.GetStatisticsAsync(request, userEmail, userRole);
                return Success(new { Success = true, Data = result, Message = "Lấy thống kê thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}