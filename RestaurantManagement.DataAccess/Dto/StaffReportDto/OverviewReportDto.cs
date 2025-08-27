using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.DataAccess.Dtos.StaffReportDto
{
    public class OverviewReportDto
    {
        public int TotalStaff { get; set; }  // Tổng số nhân viên
        public int ActiveStaffToday { get; set; }  // Nhân viên hoạt động hôm nay
        public decimal TotalHoursThisMonth { get; set; }  // Tổng giờ làm tháng này
        public decimal AttendanceRate { get; set; }  // Tỷ lệ chấm công (%)
        public int OnTimeCount { get; set; }  // Số lần đúng giờ
        public int LateCount { get; set; }  // Số lần muộn
        public List<ShiftDistributionDto> ShiftDistribution { get; set; } = new();  // Phân bố ca làm
    }

    public class ShiftDistributionDto
    {
        public string ShiftName { get; set; } = string.Empty;
        public int StaffCount { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    // DTO cho Báo cáo Chi tiết Nhân viên
    public class StaffDetailRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public Guid? StaffId { get; set; }  // Lọc theo ID nhân viên
        public string? Role { get; set; }  // Lọc theo vai trò (department)
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
    }

    public class StaffDetailResponseDto
    {
        public List<StaffDetailDto> StaffDetails { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public SummaryDto Summary { get; set; } = new();
    }

    public class StaffDetailDto
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public int TotalWorkDays { get; set; }  // Tổng ngày làm việc dự kiến
        public int ActualWorkDays { get; set; }  // Ngày làm việc thực tế
        public decimal AttendanceRate { get; set; }  // Tỷ lệ chấm công (%)
        public decimal TotalWorkedHours { get; set; }  // Tổng giờ làm
        public int OnTimeCount { get; set; }  // Số lần đúng giờ
        public int LateCount { get; set; }  // Số lần muộn
        public decimal PunctualityRate { get; set; }  // Tỷ lệ đúng giờ (%)
        public string PerformanceGrade { get; set; } = string.Empty;  // Đánh giá (Excellent, Good, v.v.)
    }

    public class SummaryDto
    {
        public decimal AverageAttendanceRate { get; set; }
        public decimal AveragePunctualityRate { get; set; }
        public int TotalStaff { get; set; }
    }
}
