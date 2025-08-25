namespace RestaurantManagement.Service.Interfaces
{
    public interface IStatisticService
    {
        Task<StatisticsResponse> GetStatisticsAsync([FromBody] StatisticsRequest request);
        Task<List<TopDishDto>> GetTopDishesAsync();
    }
}
