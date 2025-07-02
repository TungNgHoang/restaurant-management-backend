namespace RestaurantManagement.Service.Interfaces
{

    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetAllInvoiceAsync(InvoiceModels pagingModel);
    }
}
