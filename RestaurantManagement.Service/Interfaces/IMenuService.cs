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
