using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.DataAccess.Dtos.StaffReportDto;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IStaffRepository : IRepository<TblStaff>
    {
        Task<OverviewReportDto> GetOverviewReportAsync();
        Task<(List<StaffDetailDto> Details, int TotalCount)> GetStaffDetailsAsync(StaffDetailRequestDto request);
        Task<SummaryDto> GetSummaryAsync(DateTime startDate, DateTime endDate, Guid? staffId, string? role);
    }
}
