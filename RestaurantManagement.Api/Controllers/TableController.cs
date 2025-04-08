using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/Table")]
    [ApiController]
    public class TableController : BaseApiController
    {
        private readonly ITableService _tableService;
        public TableController(IServiceProvider serviceProvider, ITableService tableService) : base(serviceProvider)
        {
            _tableService = tableService;
        }

        [Authorize]
        [HttpPost("get-all-table")]
        public async Task<IActionResult> GetAllTable([FromBody] TableModels pagingModel)
        {
            var menus = await _tableService.GetAllTableAsync(pagingModel);
            var result = new PaginatedList<TableDto>(menus.ToList(), menus.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Ok(result);
        }
    }
}
