using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Dtos.ReportsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{

    public class InvoiceService : IInvoiceService
    {
        public async Task<IEnumerable<InvoiceDto>> GetAllInvoiceAsync(InvoiceModels pagingModel, string userEmail, string userRole)
        {
            // Implementation logic here  
            // Use userEmail and userRole for filtering if necessary  
            return new List<InvoiceDto>(); // Replace with actual data retrieval logic  
        }
    }
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetAllInvoiceAsync(InvoiceModels pagingModel, string userEmail, string userRole);
    }
}
