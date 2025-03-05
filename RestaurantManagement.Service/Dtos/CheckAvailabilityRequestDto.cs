using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos
{
    public class CheckAvailabilityRequestDto
    {
        public DateTime ResDate { get; set; } // Ngày và giờ đến
        public TimeOnly ResTime { get; set; } // Khoảng thời gian ở lại
        public int ResNumber { get; set; }
    }
}
