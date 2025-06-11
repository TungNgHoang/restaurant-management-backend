using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.ReportsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface ITableService
    {
        Task<IEnumerable<TableDto>> GetAllTableAsync(TableModels pagingModel);
    }
}
