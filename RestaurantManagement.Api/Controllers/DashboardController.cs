using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Interfaces;
using System;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseApiController
    {
        public IDashboardService _dashboardService { get; set; }
        public IReportService _reportService { get; set; }

        public DashboardController(IServiceProvider serviceProvider, IDashboardService dashboardService, IReportService reportService)
            : base(serviceProvider)
        {
            _dashboardService = dashboardService;
            _reportService = reportService;
        }

        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpGet("daily-report")]
        public async Task<IActionResult> GetDashboardData([FromQuery] DateTime selectedDate)
        {
            var result = await _dashboardService.GetDashboardDataAsync(selectedDate);
            return Ok(result);
        }

        [Authorize(Policy = "StaffPolicy")]
        [HttpGet("get-best-seller")]
        public async Task<IActionResult> GetBestSeller()
        {
            var result = await _dashboardService.GetTopDishesAsync();
            return Ok(result);
        }

        // Chỉ admin + ThuNgan mới được gọi endpoint này để lấy tất cả báo cáo
        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("get-all-report")]
        public async Task<IActionResult> GetAllReport([FromBody] ReportModels model)
        {
            var reportList = await _reportService.GetAllReportsAsync(model);
            var reportResult = new PaginatedList<ReportDto>(reportList.ToList(), reportList.Count(), model.PageIndex, model.PageSize);
            return Ok(reportResult);
        }
    }
}
