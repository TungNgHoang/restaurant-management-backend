using Net.payOS.Types;
using RestaurantManagement.Service.Dtos.PaymentDto;
using System.Threading.Tasks;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseApiController
    {
        public IPaymentService _paymentService { get; set; }

        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IServiceProvider serviceProvider, IPaymentService paymentService, ILogger<PaymentController> logger) : base(serviceProvider)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [Authorize(Policy = "BillingPolicy")]
        [HttpPost("checkout/{resId}")]
        public async Task<IActionResult> CheckoutAndPay(Guid resId, Guid ordId, string? proCode, [FromBody] PaymentRequestDto request)
        {
            try
            {
                await _paymentService.CheckoutAndPayAsync(resId, ordId, proCode, request.PayMethod);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        //[Authorize(Policy = "BillingPolicy")]
        //[HttpPost("payos/create/{resId}")]
        //public async Task<IActionResult> CreatePayOSPayment(Guid resId, Guid ordId, string? proCode)
        //{
        //    try
        //    {
        //        var result = await _paymentService.CreatePayOSPaymentAsync(resId, ordId, proCode);
        //        return Ok(new { Success = true, Data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Success = false, Message = ex.Message });
        //    }
        //}

        [HttpGet("success")]
        [AllowAnonymous]
        public IActionResult PaymentSuccess([FromQuery] string orderCode, [FromQuery] string paymentLinkId)
        {
            // Redirect to success page hoặc return success response
            return Ok(new { Success = true, Message = "Thanh toán thành công", OrderCode = orderCode });
        }

        
    }
}
