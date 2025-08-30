using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Interfaces; // Đảm bảo thêm namespace này

namespace RestaurantManagement.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : BaseApiController
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IServiceProvider serviceProvider, IInvoiceService invoiceService) : base(serviceProvider)
        {
            _invoiceService = invoiceService;
        }

        [Authorize(Policy = "MCPolicy")]
        [HttpPost("get-invoice")]
        public async Task<IActionResult> GetInvoice([FromBody] InvoiceModels pagingModel)
        {
            var invoices = await _invoiceService.GetAllInvoiceAsync(pagingModel);
            var listResult = new PaginatedList<InvoiceDto>(invoices.ToList(), invoices.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Success(listResult);
        }

        //[Authorize(Policy = "MCPolicy")]
        //[HttpGet("generate/{orderId}")]
        //public async Task<IActionResult> GenerateInvoice([FromRoute] Guid orderId)
        //{
        //    var pdfBytes = await _invoiceService.GenerateInvoicePdf(orderId);
        //    if (pdfBytes == null) return NotFound("Hóa đơn không tồn tại.");
        //    return File(pdfBytes, "application/pdf", $"invoice_{orderId}.pdf");
        //}
    }
}