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
        public async Task<OrderDTO> ProcessAndUpdateOrderAsync(Guid tbiId, List<OrderItemDto> newOrderItems)
        {
            // Tìm Reservation có trạng thái "Serving" cho TableID
            var reservation = await _reservationRepository
                .FindAsync(r => r.TbiId == tbiId && r.ResStatus == "Serving");

            if (reservation == null)
                throw new ErrorException(StatusCodeEnum.D02); // Không tìm thấy bàn đang phục vụ

            // Tìm OrderInfo theo ResId
            var existingOrder = await _orderInfoRepository.FindAsync(o => o.ResId == reservation.ResId);

            // Nếu chưa có order, tạo mới
            if (existingOrder == null)
            {
                var order = new TblOrderInfo
                {
                    OrdId = Guid.NewGuid(),
                    CusId = (Guid)reservation.CusId,
                    TbiId = reservation.TbiId,
                    ResId = reservation.ResId,
                    CreatedBy = Guid.Empty, // Hoặc lấy từ người dùng hiện tại
                    CreatedAt = DateTime.UtcNow,
                    TotalPrice = 0,
                    IsDeleted = false
                };

                await _orderInfoRepository.InsertAsync(order);
                existingOrder = order;
            }

            // Lấy danh sách món ăn trong đơn hàng
            var existingOrderDetails = await _orderDetailsRepository.FindListAsync(od => od.OrdId == existingOrder.OrdId) ?? new List<TblOrderDetail>();
            var existingOrderDict = existingOrderDetails.ToDictionary(od => od.MnuId);

            decimal totalPrice = existingOrder.TotalPrice;

            // Xử lý danh sách món mới
            foreach (var item in newOrderItems)
            {
                var menuItem = await _menuRepository.FindByIdAsync(item.MnuID);
                if (menuItem == null)
                    throw new ErrorException(StatusCodeEnum.D01); // Món không tồn tại

                if (existingOrderDict.TryGetValue(item.MnuID, out var existingOrderDetail))
                {
                    // Nếu món đã có, cộng dồn số lượng
                    existingOrderDetail.OdtQuantity += item.OdtQuantity;
                    await _orderDetailsRepository.UpdateAsync(existingOrderDetail);
                }
                else
                {
                    // Nếu món chưa có, thêm mới vào danh sách
                    var newOrderDetail = new TblOrderDetail
                    {
                        OdtId = Guid.NewGuid(),
                        OrdId = existingOrder.OrdId,
                        MnuId = item.MnuID,
                        OdtQuantity = item.OdtQuantity,
                        CreatedBy = existingOrder.CreatedBy,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _orderDetailsRepository.InsertAsync(newOrderDetail);
                }

                // Cập nhật tổng tiền
                totalPrice += menuItem.MnuPrice * item.OdtQuantity;
            }

            // Cập nhật tổng tiền đơn hàng
            existingOrder.TotalPrice = totalPrice;
            await _orderInfoRepository.UpdateAsync(existingOrder);

            return _mapper.Map<OrderDTO>(existingOrder);
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
                    MnuImage = menuItem.MnuImage,
                    MnuPrice = menuItem.MnuPrice,
                    OdtQuantity = detail.OdtQuantity
                });
            }

            return new OrderDetailsDto { Items = orderItemDtos };
        }

        //Tạo mới Preorder với input đầu vào là RedID 
        public async Task<OrderDTO> PreOrderOrUpdateAsync(Guid ResId, List<OrderItemDto> newOrderItems)
        {
            // Kiểm tra xem ResId có tồn tại trong bảng Reservation không
            var reservation = await _reservationRepository.FindAsync(r => r.ResId == ResId);
            if (reservation == null)
                throw new ErrorException(StatusCodeEnum.C07); // Không tìm thấy Reservation hợp lệ
            // Nếu ResId không phải là PreOrder, ném lỗi
            var alreadyOrder = await _orderInfoRepository.FindAsync(o => o.ResId == ResId && o.OrdStatus == OrderStatusEnum.Order.ToString());
            if (alreadyOrder != null)
                throw new ErrorException(StatusCodeEnum.C08); // Đã có đơn hàng với ResId này, không thể tạo PreOrder
            //Tìm trong bảng OrderInfo xem có đơn hàng nào với ResId này không
            var existingOrder = await _orderInfoRepository.FindAsync(o => o.ResId == ResId && o.OrdStatus == OrderStatusEnum.PreOrder.ToString());

            if (existingOrder == null)
            {
                // Nếu không có đơn hàng, tạo mới
                var order = new TblOrderInfo
                {
                    OrdId = Guid.NewGuid(),
                    ResId = ResId,
                    CusId = (Guid)reservation.CusId,
                    TbiId = reservation.TbiId,
                    OrdStatus = OrderStatusEnum.PreOrder.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Empty, // Hoặc lấy từ người dùng hiện tại
                    TotalPrice = 0,
                    IsDeleted = false
                };
                await _orderInfoRepository.InsertAsync(order);
                existingOrder = order;
            }

            //Lấy danh sách món ăn trong đơn hàng
            var existingOrderDetails = await _orderDetailsRepository.FindListAsync(od => od.OrdId == existingOrder.OrdId) ?? new List<TblOrderDetail>();
            var existingOrderDict = existingOrderDetails.ToDictionary(od => od.MnuId);

            decimal totalPrice = existingOrder.TotalPrice;
            // Xử lý danh sách món mới
            foreach (var item in newOrderItems)
            {
                var menuItem = await _menuRepository.FindByIdAsync(item.MnuID);
                if (menuItem == null)
                    throw new ErrorException(StatusCodeEnum.D01); // Món không tồn tại

                if (existingOrderDict.TryGetValue(item.MnuID, out var existingOrderDetail))
                {
                    // Nếu món đã có, cộng dồn số lượng
                    existingOrderDetail.OdtQuantity += item.OdtQuantity;
                    await _orderDetailsRepository.UpdateAsync(existingOrderDetail);
                }
                else
                {
                    // Nếu món chưa có, thêm mới vào danh sách
                    var newOrderDetail = new TblOrderDetail
                    {
                        OdtId = Guid.NewGuid(),
                        OrdId = existingOrder.OrdId,
                        MnuId = item.MnuID,
                        OdtQuantity = item.OdtQuantity,
                        CreatedBy = existingOrder.CreatedBy,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _orderDetailsRepository.InsertAsync(newOrderDetail);
                }

                // Cập nhật tổng tiền
                totalPrice += menuItem.MnuPrice * item.OdtQuantity;
            }
            // Cập nhật tổng tiền đơn hàng
            existingOrder.TotalPrice = totalPrice;
            await _orderInfoRepository.UpdateAsync(existingOrder);
            return _mapper.Map<OrderDTO>(existingOrder);
        }

        //Xoá mềm Order (đổi trường IsDelete thành true)
        public async Task<bool> SoftDeleteOrderAsync(Guid orderId)
        {
            var order = await _orderInfoRepository.FindByIdAsync(orderId);
            if (order == null)
                throw new ErrorException(StatusCodeEnum.D02); // Không tìm thấy đơn hàng

            // Đánh dấu đơn hàng là đã xóa trong bảng OrderInfo
            order.IsDeleted = true;
            await _orderInfoRepository.UpdateAsync(order);

            // Xoá các món ăn chứa ID đơn hàng trong bảng OrderDetails
            var orderDetails = await _orderDetailsRepository.FindListAsync(od => od.OrdId == orderId);
            foreach (var detail in orderDetails)
            {
                detail.IsDeleted = true;
                await _orderDetailsRepository.UpdateAsync(detail);
            }

            return true;
        }
    }
}
