using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.AttendanceDto
{
    public class GetAllAssignmentDto
    {
        public DateOnly WorkDate { get; set; }
        public List<ShiftSummaryDto> Shifts { get; set; } = new();
    }

    public class ShiftSummaryDto
    {
        public string shiftName { get; set; } = string.Empty; // Morning/Afternoon/Evening
        public int Count { get; set; }
        public List<StaffDto> Staffs { get; set; } = new();
    }

    public class StaffDto
    {
        public Guid staId { get; set; }
        public string staName { get; set; } = string.Empty;
    }
}
