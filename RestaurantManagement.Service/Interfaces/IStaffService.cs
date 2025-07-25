using RestaurantManagement.Service.Dtos.StaffDto;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffDto>> GetAllStaffAsync(StaffModels pagingModel);
        Task<StaffDto> GetStaffByIdAsync(Guid id);
        Task<StaffDto> AddStaffAsync(StaffDto staffDto);
        Task<StaffDto> UpdateStaffAsync(StaffDto staffDto);
        Task DeleteStaffAsync(Guid id);
    }
}
