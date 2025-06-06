using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos; // Giả sử TableModels và TableDto nằm trong namespace này
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Interfaces;
using System.Security.Claims;

namespace RestaurantManagement.Api.Controllers
{
    [Authorize]
    [Route("api/Table")]
    [ApiController]
    public class TableController : BaseApiController
    {
        private readonly ITableService _tableService;

        public TableController(IServiceProvider serviceProvider, ITableService tableService) : base(serviceProvider)
        {
            _tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
        }

        [Authorize(Roles = "admin")] // Giới hạn cho admin, hoặc điều chỉnh theo nhu cầu
        [HttpPost("get-all-table")]
        public async Task<IActionResult> GetAllTable([FromBody] TableModels pagingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Dữ liệu đầu vào không hợp lệ" });

            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                // Gửi userEmail và userRole để lọc dữ liệu nếu cần
                var tables = await _tableService.GetAllTableAsync(pagingModel, userEmail, userRole);
                var result = new PaginatedList<TableDto>(tables.ToList(), tables.Count(), pagingModel.PageIndex, pagingModel.PageSize);
                return Success(new { Success = true, Data = result, Message = "Lấy danh sách bàn thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}