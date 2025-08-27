using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.AttendanceDto
{
    public class CheckInDto
    {
        public Guid StaId { get; set; } // ID nhân viên
        public Guid AssignmentId { get; set; } // ID phân công ca làm
        public DateTime CheckInTime { get; set; } // Thời gian check-in do quản lý nhập
    }
}
