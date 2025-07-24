using RestaurantManagement.Service.Dtos.StaffDto;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffDto>> GetAllStaffAsync(StaffModels pagingModel);

    }
}
