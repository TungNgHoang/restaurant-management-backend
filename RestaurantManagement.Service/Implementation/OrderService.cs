using AutoMapper;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.OrdersDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Service.Implementation
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<TblOrderInfo> _orderInfoRepository;
        private readonly IRepository<TblOrderDetail> _orderDetailsRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblOrderInfo> orderInfoRepository,
            IRepository<TblOrderDetail> orderDetailsRepository,
            IOrderRepository orderRepository
        ) : base(appSettings, mapper) // Truyền xuống BaseService
        {
            _orderInfoRepository = orderInfoRepository;
            _orderDetailsRepository = orderDetailsRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return _mapper.Map<OrderDTO>(order);
        }
    }
}
