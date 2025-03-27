
using RestaurantManagement.Service.Dtos.ReportsDto;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(DateTime selectedDate);
    }
}
