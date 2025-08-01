namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IServiceProvider serviceProvider, IOrderService orderService) : base(serviceProvider)
        {
            _orderService = orderService;
        }

        [Authorize(Policy = "PublicAccess")]
        [HttpPost("process-order")]
        public async Task<IActionResult> ProcessOrder([FromBody] ProcessOrderRequest request)
        {
            if (request == null || request.TbiId == Guid.Empty || request.NewOrderItems == null || !request.NewOrderItems.Any())
            {
                return BadRequest(StatusCodeEnum.BadRequest);
            }

            try
            {
                var result = await _orderService.ProcessAndUpdateOrderAsync(request.TbiId, request.NewOrderItems);
                return Ok(result);
            }
            catch (ErrorException ex)
            {
                throw new ErrorException(StatusCodeEnum.Error, ex.Message);
            }
        }

        [Authorize(Policy = "StaffPolicy")]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new ErrorException(StatusCodeEnum.A02); // Đơn hàng không tồn tại

            return Ok(order);
        }

        [Authorize(Policy = "PublicAccess")]
        [HttpPost("process-preorder")]
        public async Task<IActionResult> ProcessPreOrder([FromBody] ProcessPreOrderRequest request)
        {
            if (request == null || request.ResId == Guid.Empty || request.NewOrderItems == null || !request.NewOrderItems.Any())
            {
                return BadRequest(StatusCodeEnum.BadRequest);
            }

            try
            {
                var result = await _orderService.PreOrderOrUpdateAsync(request.ResId, request.NewOrderItems);
                return Ok(result);
            }
            catch (ErrorException ex)
            {
                throw new ErrorException(StatusCodeEnum.Error, ex.Message);
            }
        }

        //Xoá mềm đơn hàng
        [Authorize(Policy = "StaffPolicy")]
        [HttpDelete("softdelete-order/{orderId}")]
        public async Task<IActionResult> DeleteOrder(Guid orderId)
        {
            var result = await _orderService.SoftDeleteOrderAsync(orderId);
            if (!result) return NotFound(new { message = StatusCodeEnum.D02 });

            return Ok(new { message = StatusCodeEnum.D04 });
        }
    }
}