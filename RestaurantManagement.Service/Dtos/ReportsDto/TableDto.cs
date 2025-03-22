using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.ReportsDto
{
    public class TableDto
    {
        public Guid TbiID { get; set; }
        public string TbiStatus { get; set; }
        public int TbiTableNumber { get; set; }
    }
}
