﻿namespace RestaurantManagement.Service.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDetailsDto> GetOrderByIdAsync(Guid orderId);
        Task<OrderDTO> ProcessAndUpdateOrderAsync(Guid tbiId, List<OrderItemDto> newOrderItems);
        Task<OrderDTO> PreOrderOrUpdateAsync(Guid ResId, List<OrderItemDto> newOrderItems);
        Task<bool> SoftDeleteOrderAsync(Guid orderId);

    }
}
