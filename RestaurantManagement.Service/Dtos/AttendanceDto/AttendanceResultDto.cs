using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.AttendanceDto
{
    public class AttendanceResultDto
    {
        public Guid AttendanceId { get; set; }
        public Guid StaId { get; set; }
        public Guid AssignmentId { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Status { get; set; } // OnTime, Late, EarlyLeave
    }
}
