using RestaurantManagement.Service.Dtos.StaffDto;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<GetStaffByIdDto>> GetAllStaffAsync(StaffModels pagingModel);
        Task<GetStaffByIdDto> GetStaffByIdAsync(Guid id);
        Task<StaffDto> AddStaffAsync(StaffDto staffDto);
        Task<UpdateStaffProfileDto> UpdateStaffProfileAsync(Guid id, UpdateStaffProfileDto staffProfileDto);
        Task DeleteStaffAsync(Guid id);
    }
}
