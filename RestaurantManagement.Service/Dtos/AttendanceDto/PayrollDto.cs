using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.AttendanceDto
{
    public class PayrollDto
    {
        public Guid PayrollId { get; set; }
        public Guid StaId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalSalary { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PayrollRequestDto
    {
        public Guid StaId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
