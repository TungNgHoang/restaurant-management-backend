using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.StaffDto
{
    public class StaffDto
    {
        public Guid StaID { get; set; }
        public Guid UacID { get; set; }
        public string StaName { get; set; }
        public string StaRole { get; set; }
        public string StaPhone { get; set; }
        public string StaBaseSalary { get; set; }
    }
}
