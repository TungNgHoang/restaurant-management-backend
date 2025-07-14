namespace RestaurantManagement.Service.Interfaces
{
    public interface IReportService
    {
        Task<List<ReportDto>> GetAllReportsAsync(ReportModels model);
    }
}
