using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/Menu")]
    [ApiController]
    public class MenuController : BaseApiController
    {
        private readonly IMenuService _menuService;

        public MenuController(IServiceProvider serviceProvider, IMenuService menuService) : base(serviceProvider)
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

        // Lấy thông tin món theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuById(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null) return NotFound();
            return Ok(menu);
        }


        [HttpPost("add_item_to_menu")]
        public async Task<IActionResult> AddMenu([FromBody] MenuDto menuDto)
        {
            if (menuDto == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            var newMenu = await _menuService.AddMenuAsync(menuDto);
            return Ok(newMenu);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] MenuDto menuDto)
        {
            var updatedMenu = await _menuService.UpdateMenuAsync(id, menuDto);
            if (updatedMenu == null) return NotFound();
            return Ok(updatedMenu);
        }

        [HttpDelete("softdelete_item/{id}")]
        public async Task<IActionResult> DeleteMenu(Guid id)
        {
            var result = await _menuService.DeleteMenuAsync(id);
            if (!result) return NotFound(new { message = "Menu not found" });

            return Ok(new { message = "Deleted successfully" });
        }
    }
}
