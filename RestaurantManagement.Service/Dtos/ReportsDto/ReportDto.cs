using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.ReportsDto
{
    public class ReportDto
    {
        public string ReportDay { get; set; }
        public string ReportMonth { get; set; }
        public string ReportYear { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDishesSold { get; set; }
        public int TotalReservations { get; set; }
        public int TotalCustomers { get; set; }
        public string BestSellingDish { get; set; }

    }
}
