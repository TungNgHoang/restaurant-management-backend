using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuDto>> GetAllMenuAsync(MenuModels pagingModel);
        Task<MenuDto> AddMenuAsync(MenuDto menuDto);
        Task<MenuDto> UpdateMenuAsync(Guid id, MenuDto menuDto);
        Task<bool> DeleteMenuAsync(Guid id);
        Task<MenuDto> GetMenuByIdAsync(Guid id);
    }
}
