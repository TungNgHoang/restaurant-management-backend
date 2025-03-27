using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class DashboardService : BaseService, IDashboardService
    {
        private readonly IMapper _mapper;
        protected readonly RestaurantDBContext _dbContext;
        private readonly IRepository<TblOrderInfo> _orderRepository;
        private readonly IRepository<TblReservation> _reservationRepository;
        private readonly IRepository<TblCustomer> _customerRepository;
        private readonly IRepository<TblPayment> _paymentRepository;
        public DashboardService(
            AppSettings appSettings,
            IMapper mapper,
            RestaurantDBContext dbContext,
            IRepository<TblOrderInfo> orderRepository,
            IRepository<TblCustomer> customerRepository,
            IRepository<TblReservation> reservationRepository,
            IRepository<TblPayment> paymentRepository
            ) : base(appSettings, mapper)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _reservationRepository = reservationRepository;
            _customerRepository = customerRepository;
            _paymentRepository = paymentRepository;
            _dbContext = dbContext;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(DateTime selectedDate)
        {
            DateTime previousDate = selectedDate.AddDays(-1);

            // 1. Lấy danh sách Reservation theo ngày
            var reservationsToday = await _reservationRepository.FilterAsync(r => r.ResDate.Date == selectedDate.Date);
            var reservationsYesterday = await _reservationRepository.FilterAsync(r => r.ResDate.Date == previousDate.Date);

            // 2. Tổng số lượt đặt bàn
            var reservationCountToday = reservationsToday.Count();
            var reservationCountYesterday = reservationsYesterday.Count();

            // 3. Tổng doanh thu
            var totalRevenueToday = await _dbContext.TblPayments
                .Where(p => p.CreatedAt.Date == selectedDate.Date) // Lọc theo ngày hôm nay
                .SumAsync(p => p.Amount); // Tính tổng Amount
            var totalRevenueYesterday = await _dbContext.TblPayments
                .Where(p => p.CreatedAt.Date == previousDate.Date) // Lọc theo ngày hôm trước
                .SumAsync(p => p.Amount); // Tính tổng Amount

            // Tổng số món ăn đã bán trong ngày được chọn
            var dishesCountToday = await _dbContext.TblOrderDetails
                .Where(o => o.CreatedAt >= selectedDate.Date && o.CreatedAt < selectedDate.AddDays(1))
                .SumAsync(o => (int?)o.OdtQuantity) ?? 0;

            // Tổng số món ăn đã bán trong ngày hôm trước
            var dishesCountYesterday = await _dbContext.TblOrderDetails
                .Where(o => o.CreatedAt >= previousDate.Date && o.CreatedAt < previousDate.AddDays(1))
                .SumAsync(o => (int?)o.OdtQuantity) ?? 0;


            // 6. Tổng số khách hàng mới
            var newCustomersToday = await _dbContext.TblCustomers
                .Where(c => c.CreatedAt.Date == selectedDate.Date)
                .CountAsync();

            var newCustomersYesterday = await _dbContext.TblCustomers
                .Where(c => c.CreatedAt.Date == previousDate.Date)
                .CountAsync();

            // 7. Tính toán phần trăm thay đổi
            decimal revenueChange = 0; // Khởi tạo giá trị mặc định

            if (totalRevenueYesterday > 0)
            {
                if (totalRevenueToday == 0)
                {
                    revenueChange = -100; // Hôm nay = 0, hôm qua > 0 → giảm 100%
                }
                else
                {
                    revenueChange = ((totalRevenueToday - totalRevenueYesterday) / totalRevenueYesterday) * 100;
                }
            }
            else if (totalRevenueYesterday == 0)
            {
                revenueChange = (totalRevenueToday > 0) ? 100 : 0;
            }

            decimal reservationsChange = 0; // Khởi tạo giá trị mặc định

            if (reservationCountYesterday > 0)
            {
                if (reservationCountToday == 0)
                {
                    reservationsChange = -100; // Hôm nay = 0, hôm qua > 0 → giảm 100%
                }
                else
                {
                    reservationsChange = ((reservationCountToday - reservationCountYesterday) / (decimal)reservationCountYesterday) * 100;
                }
            }
            else if (reservationCountYesterday == 0)
            {
                reservationsChange = (reservationCountToday > 0) ? 100 : 0;
            }
            decimal dishesChange = 0; // Khởi tạo giá trị mặc định

            if (dishesCountYesterday > 0)
            {
                if (dishesCountToday == 0)
                {
                    dishesChange = -100; // Hôm nay = 0, hôm qua > 0 → giảm 100%
                }
                else
                {
                    dishesChange = ((dishesCountToday - dishesCountYesterday) / (decimal)dishesCountYesterday) * 100;
                }
            }
            else if (dishesCountYesterday == 0)
            {
                dishesChange = (dishesCountToday > 0) ? 100 : 0;
            }
            decimal customersChange = 0; // Khởi tạo giá trị mặc định

            if (newCustomersYesterday > 0)
            {
                if (newCustomersToday == 0)
                {
                    customersChange = -100; // Hôm nay = 0, hôm qua > 0 → giảm 100%
                }
                else
                {
                    customersChange = ((newCustomersToday - newCustomersYesterday) / (decimal)newCustomersYesterday) * 100;
                }
            }
            else if (newCustomersYesterday == 0)
            {
                customersChange = (newCustomersToday > 0) ? 100 : 0;
            }


            // 8. Trả về dữ liệu dashboard
            return new DashboardDto
            {
                TotalRevenue = totalRevenueToday,
                RevenueChangePercentage = revenueChange,
                TotalDishesSold = dishesCountToday,
                DishesChangePercentage = dishesChange,
                TotalReservations = reservationCountToday,
                ReservationsChangePercentage = reservationsChange,
                TotalCustomers = newCustomersToday,
               CustomersChangePercentage = customersChange
            };

        }
    }
}
