using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Service.Dtos.OrdersDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IServiceProvider serviceProvider, IOrderService orderService) : base(serviceProvider)
        {
            _orderService = orderService;
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPost("process-order")]
        public async Task<IActionResult> ProcessOrder([FromBody] ProcessOrderRequest request)
        {
            if (request == null || request.TbiId == Guid.Empty || request.NewOrderItems == null || !request.NewOrderItems.Any())
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var result = await _orderService.ProcessAndUpdateOrderAsync(request.TbiId, request.NewOrderItems);
                return Ok(result);
            }
            catch (ErrorException ex)
            {
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
        }

        [Authorize(Policy = "AdminManagerUserPolicy")]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new ErrorException(StatusCodeEnum.A02); // Đơn hàng không tồn tại

            return Ok(order);
        }

        public class ProcessOrderRequest
        {
            public Guid TbiId { get; set; }
            public List<OrderItemDto> NewOrderItems { get; set; }
        }
    }
}