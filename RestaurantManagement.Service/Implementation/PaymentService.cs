using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class PaymentService : BaseService, IPaymentService
    {
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblTableInfo> _tablesRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<TblPayment> _paymentRepository;
        private readonly IRepository<TblPromotion> _promotionRepository;
        protected readonly RestaurantDBContext _dbContext;
        public PaymentService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblReservation> reservationsRepository,
            IRepository<TblTableInfo> tablesRepository,
            IOrderRepository orderRepository,
            IRepository<TblPayment> paymentRepositor,
            IRepository<TblPromotion> promotionRepository,
            RestaurantDBContext dbContext
            ) : base(appSettings, mapper)
        {
            _dbContext = dbContext;
            _reservationsRepository = reservationsRepository;
            _tablesRepository = tablesRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepositor;
            _promotionRepository = promotionRepository;
        }

        public async Task CheckoutAndPayAsync(Guid resId, Guid ordId, string proCode, string payMethod)
        {
            // 1. Lấy thông tin đơn hàng
            var order = await _orderRepository.GetOrderByIdAsync(ordId);
            if (order == null)
                throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

            // 1. Kiểm tra reservation
            var reservation = await _reservationsRepository.FindByIdAsync(resId);
            if (reservation == null || reservation.ResStatus != ReservationStatus.Serving.ToString())
                throw new ErrorException(StatusCodeEnum.A03);

            // 2. Kiểm tra bàn
            var table = await _tablesRepository.FindByIdAsync(reservation.TbiId);
            if (table == null || table.TbiStatus != TableStatus.Occupied.ToString())
                throw new ErrorException(StatusCodeEnum.A04);

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 4. Tạo bản ghi thanh toán
                    var payment = new TblPayment
                    {
                        PayId = Guid.NewGuid(),
                        OrdId = order.OrdId,
                        CusId = order.CusId,
                        Amount = order.TotalPrice,
                        PayMethod = payMethod,
                        PayStatus = "Completed",
                        IsDeleted = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = Guid.Empty // Thay bằng ID nhân viên nếu cần
                    };
                    await _paymentRepository.InsertAsync(payment);

                    // 5. Cập nhật trạng thái reservation
                    reservation.ResStatus = ReservationStatus.Finished.ToString() /*"Finished",*/;
                    reservation.UpdatedAt = DateTime.Now;
                    reservation.UpdatedBy = Guid.Empty; // Thay bằng ID nhân viên nếu cần
                    await _reservationsRepository.UpdateAsync(reservation);

                    // 6. Cập nhật trạng thái bàn
                    table.TbiStatus = TableStatus.Empty.ToString(); //"Empty"
                    table.UpdatedAt = DateTime.Now;
                    table.UpdatedBy = Guid.Empty; // Thay bằng ID nhân viên nếu cần
                    await _tablesRepository.UpdateAsync(table);

                    // Nếu cả hai thành công, commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Lỗi khi check-out: {ex.Message}");
                }
            }
        }
    }
}
