using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.Dtos.OrdersDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 1. Tạo đơn hàng
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            if (request == null || request.OrderDetails == null || !request.OrderDetails.Any())
            {
                return BadRequest("Thông tin đơn hàng không hợp lệ.");
            }

            var order = await _orderService.CreateOrderAsync(request);
            return Ok(new { Message = "Tạo đơn hàng thành công!", OrderId = order.OrdID });
        }

        //// 2. Xem đơn hàng vừa tạo
        //[HttpGet("{orderId}")]
        //public async Task<IActionResult> GetOrderById(Guid orderId)
        //{
        //    var result = await _orderService.GetOrderByIdAsync(orderId);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(result);
        //}

        //// 3. Cập nhật thêm món vào đơn hàng
        //[HttpPost("{orderId}/add-item")]
        //public async Task<IActionResult> AddItemToOrder(Guid orderId, [FromBody] AddItemToOrderRequestDto request)
        //{
        //    var result = await _orderService.AddItemToOrderAsync(orderId, request);
        //    return Ok(result);
        //}
    }
}
