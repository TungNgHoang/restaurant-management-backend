using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.ReportsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{

    public class TableService : ITableService
    {
        public async Task<IEnumerable<TableDto>> GetAllTableAsync(TableModels pagingModel, string userEmail, string userRole)
        {
            // Implementation logic here  
            // Use userEmail and userRole for filtering if needed  
            return new List<TableDto>(); // Replace with actual data retrieval logic  
        }
    }
    public interface ITableService
    {
        Task<IEnumerable<TableDto>> GetAllTableAsync(TableModels pagingModel, string userEmail, string userRole);
    }
}
