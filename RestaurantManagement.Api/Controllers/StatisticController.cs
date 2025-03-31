using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : BaseApiController
    {
        private readonly IStatisticService _statisticService;
        public StatisticController(IServiceProvider serviceProvider, IStatisticService statisticService) : base(serviceProvider)
        {
            _statisticService = statisticService;
        }

        [HttpPost("get-statistic")]
        public async Task<IActionResult> GetStatistics([FromBody] StatisticsRequest request)
        {
            var result = await _statisticService.GetStatisticsAsync(request);
            return Ok(result);
        }
    }
}
