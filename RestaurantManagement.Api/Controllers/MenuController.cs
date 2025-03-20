using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/Menu")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpPost("get_all")]
        public async Task<IActionResult> GetAllMenu([FromBody] MenuModels pagingModel)
        {
            var menus = await _menuService.GetAllMenuAsync(pagingModel);
            var result = new PaginatedList<MenuDto>(menus.ToList(), menus.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Ok(result);
        }
    }
}
