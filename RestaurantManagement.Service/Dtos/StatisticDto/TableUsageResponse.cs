namespace RestaurantManagement.Service.Dtos.StatisticDto
{
    public class TableUsageDto
    {
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public int ReservationCount { get; set; }
        public int OrderCount { get; set; }
        public decimal ConversionRate { get; set; } // % chuyển đổi từ reservation thành order
        public decimal UsageRate { get; set; } // % so với tổng order của tất cả bàn
        public string Status { get; set; }
    }

    public class TableUsageResponse
    {
        public string Month { get; set; } // Format: "MM/yyyy"
        public int TotalDays { get; set; }
        public int TotalTables { get; set; }
        public int TablesUsed { get; set; } // Số bàn có ít nhất 1 order
        public decimal AverageUsageRate { get; set; } // Số bàn được sử dụng / tổng số bàn
        public int TotalOrders { get; set; } // Tổng số order của tất cả bàn
        public List<TableUsageDto> Tables { get; set; } = new List<TableUsageDto>();
    }

}
