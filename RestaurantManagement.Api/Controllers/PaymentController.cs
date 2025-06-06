using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.PaymentDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseApiController
    {
        public IPaymentService _paymentService { get; set; }
     
        public PaymentController(IServiceProvider serviceProvider, IPaymentService paymentService) : base(serviceProvider)
        {
            _paymentService = paymentService;

        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPost("checkout/{resId}")]
        public async Task<IActionResult> CheckoutAndPay(Guid resId, Guid ordId, [FromBody] PaymentRequestDto request)
        {
            try
            {
                await _paymentService.CheckoutAndPayAsync(resId, ordId, request.PayMethod);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }


    }
}
