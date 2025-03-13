using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.ReserDto
{
    public class AvailableTableDto
    {
        public Guid TbiId { get; set; }
        public int TbiTableNumber { get; set; }
    }
}
