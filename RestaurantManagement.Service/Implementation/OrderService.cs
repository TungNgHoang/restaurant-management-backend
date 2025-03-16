using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.OrderDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDTO> CreateOrderAsync(OrderDTO orderDto)
        {
            var order = new TblOrderInfo
            {
                OrdId = Guid.NewGuid(),
                CusId = orderDto.CusID,
                TbiId = orderDto.TblID,
                TotalPrice = orderDto.OrderDetails.Sum(d => d.Price * d.OdtQuantity),
                CreatedAt = DateTime.UtcNow,
            };

            var orderDetails = orderDto.OrderDetails.Select(d => new TblOrderDetail
            {
                OdtId = Guid.NewGuid(),
                MnuId = d.MnuID,
                OdtQuantity = d.OdtQuantity,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            var createdOrder = await _orderRepository.CreateOrderAsync(order, orderDetails);

            return new OrderDTO
            {
                OrdID = createdOrder.OrdId,
                CusID = createdOrder.CusId,
                TblID = createdOrder.TbiId,
                TotalPrice = createdOrder.TotalPrice,
                OrderDetails = orderDetails.Select(d => new OrderDetailDTO
                {
                    OdtID = d.OdtId,
                    MnuID = d.MnuId,
                    OdtQuantity = d.OdtQuantity
                }).ToList()
            };
        }
    }
}
