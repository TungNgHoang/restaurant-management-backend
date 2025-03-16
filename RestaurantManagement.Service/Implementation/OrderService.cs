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
        private readonly IRepository<TblMenu> _menuRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblOrderInfo> orderInfoRepository,
            IRepository<TblOrderDetail> orderDetailsRepository,
            IRepository<TblMenu> menuRepository,
            IOrderRepository orderRepository
        ) : base(appSettings, mapper) // Truyền xuống BaseService
        {
            _orderInfoRepository = orderInfoRepository;
            _orderDetailsRepository = orderDetailsRepository;
            _menuRepository = menuRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderRequestDto request)
        {
            var order = new TblOrderInfo
            {
                OrdId = Guid.NewGuid(),
                CusId = request.CusId,
                TbiId = request.TbiId,
                ResId = request.ResId,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0,
                IsDeleted = false
            };

            await _orderInfoRepository.InsertAsync(order);

            decimal totalPrice = 0;
            var orderDetails = new List<TblOrderDetail>();

            foreach (var item in request.OrderDetails)
            {
                var menuItem = await _menuRepository.FindByIdAsync(item.MnuId);
                if (menuItem == null)
                    throw new Exception($"Món ăn với ID {item.MnuId} không tồn tại.");

                var orderDetail = new TblOrderDetail
                {
                    OdtId = Guid.NewGuid(),
                    OrdId = order.OrdId,
                    MnuId = item.MnuId,
                    OdtQuantity = item.OdtQuantity,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                orderDetails.Add(orderDetail);
                totalPrice += menuItem.MnuPrice * item.OdtQuantity;
            }

            await _orderDetailsRepository.InsertManyAsync(orderDetails);
            order.TotalPrice = totalPrice;

            await _orderInfoRepository.UpdateAsync(order);
            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return _mapper.Map<OrderDTO>(order);
        }
    }
}
