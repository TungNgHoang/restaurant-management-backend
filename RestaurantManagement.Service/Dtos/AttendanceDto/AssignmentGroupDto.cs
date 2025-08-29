using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.AttendanceDto
{
    public class AssignmentGroupDto
    {
        public DateOnly WorkDate { get; set; }
        public List<AssignmentDetailDto> Assignments { get; set; } = new();
    }

    public class AssignmentDetailDto
    {
        public Guid AssignmentId { get; set; }
        public Guid ShiftId { get; set; }
        public Guid StaId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string staName { get; set; } = string.Empty;
        public string staRole { get; set; } = string.Empty;
    }
}
