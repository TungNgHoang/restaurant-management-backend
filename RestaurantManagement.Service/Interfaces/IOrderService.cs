using RestaurantManagement.Service.Dtos.OrdersDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDetailsDto> GetOrderByIdAsync(Guid orderId);
        Task<OrderDTO> ProcessAndUpdateOrderAsync(Guid tbiId, List<OrderItemDto> newOrderItems);
        Task<OrderDTO> PreOrderOrUpdateAsync(Guid ResId, List<OrderItemDto> newOrderItems);

    }
}
