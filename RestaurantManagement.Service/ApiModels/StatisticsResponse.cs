using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.ApiModels
{
    public class StatisticsResponse
    {
        public string Period { get; set; }
        public List<StatisticsData> Data { get; set; }
    }

    public class StatisticsData
    {
        public string Time { get; set; }
        public decimal Revenue { get; set; }
        public int Customers { get; set; }
        public int Dishes { get; set; }
        public int Reservations { get; set; }
    }
}
