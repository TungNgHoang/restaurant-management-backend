using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{

    public class StatisticService : IStatisticService
    {
        public async Task<StatisticsResponse> GetStatisticsAsync(StatisticsRequest request, string userEmail, string userRole)
        {
            // Implementation logic here  
            throw new NotImplementedException();
        }
    }
    public interface IStatisticService
    {
        Task<StatisticsResponse> GetStatisticsAsync(StatisticsRequest request, string userEmail, string userRole);
    }
}
