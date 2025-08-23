using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.ApiModels;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetAllInvoiceAsync(InvoiceModels pagingModel);
        Task<byte[]> GenerateInvoicePdf(Guid orderId);
    }
}