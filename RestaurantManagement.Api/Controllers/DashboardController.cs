using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseApiController
    {
        public IDashboardService _dashboardService { get; set; }
        public DashboardController(IServiceProvider serviceProvider, IDashboardService dashboardService) : base(serviceProvider)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData([FromQuery] DateTime selectedDate)
        {
            var result = await _dashboardService.GetDashboardDataAsync(selectedDate);
            return Ok(result);
        }
    }
}
