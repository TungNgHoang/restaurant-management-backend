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

        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("get-statistic")]
        public async Task<IActionResult> GetStatistics([FromBody] StatisticsRequest request)
        {
            var result = await _statisticService.GetStatisticsAsync(request);
            return Ok(result);
        }
    }
}