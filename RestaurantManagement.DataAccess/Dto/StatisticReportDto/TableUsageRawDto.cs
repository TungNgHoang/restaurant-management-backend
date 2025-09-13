namespace RestaurantManagement.DataAccess.Dto.StatisticReportDto
{
    public class TableUsageRawDto
    {
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public int ReservationCount { get; set; }
        public int OrderCount { get; set; }
    }
}
