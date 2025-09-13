using RestaurantManagement.Service.Dtos.StatisticDto;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IStatisticService
    {
        Task<StatisticsResponse> GetStatisticsAsync([FromBody] StatisticsRequest request);
        Task<List<TopDishDto>> GetTopDishesAsync();
        Task<TableUsageResponse> GetTableUsageReportAsync(int month, int year);
    }
}
