using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos
{
    public class AvailableTableDto
    {
        public Guid TbiId { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
    }
}
