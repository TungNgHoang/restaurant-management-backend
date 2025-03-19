using AutoMapper;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.Implementation;
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
        private readonly IRepository<TblReservation> _reservationRepository;
        private readonly IRepository<TblMenu> _menuRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblOrderInfo> orderInfoRepository,
            IRepository<TblOrderDetail> orderDetailsRepository,
            IRepository<TblMenu> menuRepository,
            IOrderRepository orderRepository,
            IRepository<TblReservation> reservationRepository
        ) : base(appSettings, mapper) // Truyền xuống BaseService
        {
            _orderInfoRepository = orderInfoRepository;
            _orderDetailsRepository = orderDetailsRepository;
            _menuRepository = menuRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _reservationRepository = reservationRepository;
        }
        public async Task<OrderDTO> ProcessOrderAsync(CreateOrderRequestDto request)
        {
            // Tìm đơn hàng dựa trên ResId và TbiId
            var order = await _orderInfoRepository.FindAsync(o => o.ResId == request.ResId && o.TbiId == request.TbiId && !o.IsDeleted);

            if (order == null)
            {
                // Trường hợp chưa có đơn hàng: Tạo mới
                order = new TblOrderInfo
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
            }

            // Lấy danh sách chi tiết đơn hàng hiện có (nếu có)
            var existingOrderDetails = await _orderDetailsRepository.FindListAsync(od => od.OrdId == order.OrdId);
            var existingOrderDict = existingOrderDetails.ToDictionary(od => od.MnuId); // Tạo dictionary để tra cứu nhanh

            decimal totalPrice = order.TotalPrice; // Bắt đầu từ tổng giá hiện tại

            // Duyệt qua từng món trong request
            foreach (var item in request.OrderDetails)
            {
                var menuItem = await _menuRepository.FindByIdAsync(item.MnuId);
                if (menuItem == null)
                    throw new ErrorException(StatusCodeEnum.D01); // Món không tồn tại

                if (existingOrderDict.TryGetValue(item.MnuId, out var existingOrderDetail))
                {
                    // Nếu món đã tồn tại, cộng dồn số lượng
                    existingOrderDetail.OdtQuantity += item.OdtQuantity;
                    totalPrice += menuItem.MnuPrice * item.OdtQuantity; // Cộng thêm vào tổng giá
                    await _orderDetailsRepository.UpdateAsync(existingOrderDetail);
                }
                else
                {
                    // Nếu món chưa tồn tại, tạo mới chi tiết đơn hàng
                    var newOrderDetail = new TblOrderDetail
                    {
                        OdtId = Guid.NewGuid(),
                        OrdId = order.OrdId,
                        MnuId = item.MnuId,
                        OdtQuantity = item.OdtQuantity,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _orderDetailsRepository.InsertAsync(newOrderDetail);
                    totalPrice += menuItem.MnuPrice * item.OdtQuantity; // Cộng vào tổng giá
                }
            }

            // Cập nhật tổng giá cho đơn hàng
            order.TotalPrice = totalPrice;
            await _orderInfoRepository.UpdateAsync(order);

            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDetailsDto> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderDetailsRepository.FindListAsync(od => od.OrdId == orderId);
            if (order == null || !order.Any())
                throw new ErrorException(StatusCodeEnum.D02);
            var orderItemDtos = new List<OrderItem>();
            foreach (var detail in order)
            {
                // Nếu navigation property không được load, bạn có thể lấy từ repository:
                TblMenu menuItem;
                if (detail.Mnu == null)
                {
                    menuItem = await _menuRepository.FindByIdAsync(detail.MnuId);
                }
                else
                {
                    menuItem = detail.Mnu;
                }

                if (menuItem == null)
                {
                    // Nếu không tìm thấy thông tin món ăn, ném exception
                    throw new ErrorException(StatusCodeEnum.D01);
                }

                orderItemDtos.Add(new OrderItem
                {
                    MnuId = detail.MnuId,
                    MnuName = menuItem.MnuName,
                    MnuPrice = menuItem.MnuPrice,
                    OdtQuantity = detail.OdtQuantity
                });
            }

            return new OrderDetailsDto { Items = orderItemDtos };
        }
    }
}
