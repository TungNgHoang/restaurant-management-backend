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
    
    [Route("api/Table")]
    [ApiController]
    public class TableController : BaseApiController
    {
        private readonly ITableService _tableService;

        public TableController(IServiceProvider serviceProvider, ITableService tableService) : base(serviceProvider)
        {
            _tableService = tableService;

        }

        
        [HttpPost("get-all-table")]
        public async Task<IActionResult> GetAllTable([FromBody] TableModels pagingModel)
        {
            var menus = await _tableService.GetAllTableAsync(pagingModel);
            var result = new PaginatedList<TableDto>(menus.ToList(), menus.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Ok(result);
        }
    }
}