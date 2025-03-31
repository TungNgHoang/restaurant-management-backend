using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IStatisticService
    {
        Task<StatisticsResponse> GetStatisticsAsync([FromBody] StatisticsRequest request);
    }
}
