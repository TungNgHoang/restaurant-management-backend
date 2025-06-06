using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos; // Giả sử InvoiceModels và InvoiceDto nằm trong namespace này
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Interfaces;
using System.Security.Claims;

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
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        }

        [Authorize(Policy = "AdminManagerPolicy")]
        [HttpPost("get-invoice")]
        public async Task<IActionResult> GetInvoice([FromBody] InvoiceModels pagingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Dữ liệu đầu vào không hợp lệ" });

            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                // Gửi userEmail và userRole để lọc dữ liệu nếu cần
                var invoices = await _invoiceService.GetAllInvoiceAsync(pagingModel, userEmail, userRole);
                var listResult = new PaginatedList<InvoiceDto>(invoices.ToList(), invoices.Count(), pagingModel.PageIndex, pagingModel.PageSize);
                return Success(new { Success = true, Data = listResult, Message = "Lấy danh sách hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}