using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : BaseApiController
    {
        private readonly IMenuService _menuService;

        public MenuController(IServiceProvider serviceProvider, IMenuService menuService) : base(serviceProvider)
        {
            _menuService = menuService;
        }

        //Cho phép user và admin xem menu
        [Authorize(Policy = "PublicAccess")]
        [HttpPost("get-all-menu")]
        public async Task<IActionResult> GetAllMenu([FromBody] MenuModels pagingModel)
        {
            var menus = await _menuService.GetAllMenuAsync(pagingModel);
            var result = new PaginatedList<MenuDto>(menus.ToList(), menus.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Ok(result);
        }

        //Cho phép user và admin xem chi tiết món
        [Authorize(Policy = "PublicAccess")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuById(Guid id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null) return NotFound();
            return Ok(menu);
        }

        //Chỉ admin được thêm món
        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("add-item-to-menu")]
        public async Task<IActionResult> AddMenu([FromBody] MenuDto menuDto)
        {
            if (menuDto == null)
                return BadRequest(StatusCodeEnum.BadRequest);

            var newMenu = await _menuService.AddMenuAsync(menuDto);
            return Ok(newMenu);
        }

        //Chỉ admin được cập nhật món
        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] MenuDto menuDto)
        {
            var updatedMenu = await _menuService.UpdateMenuAsync(id, menuDto);
            if (updatedMenu == null) return NotFound();
            return Ok(updatedMenu);
        }

        //Chỉ admin được xoá mềm món
        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpDelete("softdelete-item/{id}")]
        public async Task<IActionResult> DeleteMenu(Guid id)
        {
            var result = await _menuService.DeleteMenuAsync(id);
            if (!result) return NotFound(new { message = StatusCodeEnum.D01 });

            return Ok(new { message = StatusCodeEnum.D03 });
        }
    }
}