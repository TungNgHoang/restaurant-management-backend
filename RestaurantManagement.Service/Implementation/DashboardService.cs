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

            // Hàm tính phần trăm thay đổi
            decimal CalculatePercentageChange(decimal todayValue, decimal yesterdayValue)
            {
                decimal change = 0;

                if (yesterdayValue > 0)
                {
                    if (todayValue == 0)
                    {
                        change = -100; // Hôm nay = 0, hôm qua > 0 → giảm 100%
                    }
                    else
                    {
                        change = ((todayValue - yesterdayValue) / yesterdayValue) * 100;
                    }
                }
                else if (yesterdayValue == 0)
                {
                    change = (todayValue > 0) ? 100 : 0;
                }

                return change;
            }

            // Sử dụng hàm cho 4 trường hợp
            decimal revenueChange = CalculatePercentageChange(totalRevenueToday, totalRevenueYesterday);
            decimal reservationsChange = CalculatePercentageChange(reservationCountToday, reservationCountYesterday);
            decimal dishesChange = CalculatePercentageChange(dishesCountToday, dishesCountYesterday);
            decimal customersChange = CalculatePercentageChange(newCustomersToday, newCustomersYesterday);



            // 8. Trả về dữ liệu dashboard
            var dashboard = new DashboardDto
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

            return dashboard;

        }

        public async Task<List<TopDishDto>> GetTopDishesAsync()
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime monthAgo = today.AddDays(-30);
            DateTime twoMonthsAgo = monthAgo.AddDays(-30);

            // Lấy dữ liệu bán hàng trong 7 ngày gần nhất
            var recentSales = await _dbContext.TblOrderDetails
                .Where(od => od.CreatedAt >= monthAgo && od.CreatedAt < today)
                .GroupBy(od => od.MnuId)
                .Select(g => new { MnuId = g.Key, Quantity = g.Sum(od => od.OdtQuantity) })
                .ToListAsync();

            // Lấy dữ liệu bán hàng trong 7 ngày trước đó
            var previousSales = await _dbContext.TblOrderDetails
                .Where(od => od.CreatedAt >= twoMonthsAgo && od.CreatedAt < monthAgo)
                .GroupBy(od => od.MnuId)
                .Select(g => new { MnuId = g.Key, Quantity = g.Sum(od => od.OdtQuantity) })
                .ToListAsync();

            // Lấy tên món ăn
            var menuItems = await _dbContext.TblMenus.ToListAsync();

            // Tính toán phần trăm tăng trưởng và chọn top 5
            var topDishes = recentSales
                .Select(rs => {
                    var prevQuantity = previousSales.FirstOrDefault(ps => ps.MnuId == rs.MnuId)?.Quantity ?? 0;
                    decimal growth = prevQuantity == 0 ? (rs.Quantity > 0 ? 100 : 0) : ((decimal)(rs.Quantity - prevQuantity) / prevQuantity) * 100;
                    var menuItem = menuItems.FirstOrDefault(m => m.MnuId == rs.MnuId);
                    return new TopDishDto
                    {
                        MnuId = rs.MnuId,
                        MnuName = menuItem?.MnuName ?? "Unknown",
                        MnuImage = menuItem?.MnuImage ?? "No Image",
                        MnuPrice = menuItem?.MnuPrice ?? 0,
                        QuantitySold = rs.Quantity,
                        GrowthPercentage = growth
                    };
                })
                .OrderByDescending(d => d.QuantitySold)
                .Take(5)
                .ToList();

            return topDishes;
        }
    }
}
