using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Service.Dtos.OrdersDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/Orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 1. Lựa chọn Tạo mới Hay Cập nhật
        [HttpPost("process-order")]
        public async Task<IActionResult> ProcessOrder([FromBody] CreateOrderRequestDto request)
        {
            try
            {
                var result = await _orderService.ProcessOrderAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. Xem đơn hàng vừa tạo
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new ErrorException(StatusCodeEnum.A02); // Đơn hàng không tồn tại

            return Ok(order);
        }
    }
}
