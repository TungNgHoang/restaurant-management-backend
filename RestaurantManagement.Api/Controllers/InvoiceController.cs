using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Dtos.ReserDto;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : BaseApiController
    {
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(IServiceProvider serviceProvider, IInvoiceService invoiceService) : base(serviceProvider)
        {
            _invoiceService = invoiceService;
        }

        
        [HttpPost("get-invoice")]
        public async Task<IActionResult> GetInvoice([FromBody] InvoiceModels pagingModel)
        {
            var invoices = await _invoiceService.GetAllInvoiceAsync(pagingModel);
            var listResult = new PaginatedList<InvoiceDto>(invoices.ToList(), invoices.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Success(listResult);
        }
    }
}
