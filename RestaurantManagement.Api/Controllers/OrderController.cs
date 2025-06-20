using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Service.Dtos.OrdersDto;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Service.Interfaces;
using static RestaurantManagement.Service.ApiModels.OrderRequest;

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

        [Authorize(Policy = "AdminManagerUserPolicy")]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                throw new ErrorException(StatusCodeEnum.A02); // Đơn hàng không tồn tại

            return Ok(order);
        }

        [Authorize(Policy = "UserPolicy")]
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
        [Authorize(Policy = "UserPolicy")]
        [HttpDelete("softdelete-order/{orderId}")]
        public async Task<bool> DeleteOrder(Guid orderId)
        {
            var result = await _orderService.SoftDeleteOrderAsync(orderId);
            if (!result) throw new ErrorException(StatusCodeEnum.D01); // Đơn hàng không tồn tại

            return true;
        }
    }
}