
using RestaurantManagement.Service.Dtos.StatisticDto;

namespace RestaurantManagement.Service.Implementation
{
    public class StatisticService : BaseService, IStatisticService
    {
        private readonly IStatisticRepository _statisticRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly RestaurantDBContext _context;
        public StatisticService(AppSettings appSettings, IMapper mapper, IStatisticRepository statisticRepository, IHttpContextAccessor httpContextAccessor, RestaurantDBContext context) : base(appSettings, mapper, httpContextAccessor, context)
        {
            _statisticRepository = statisticRepository;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<StatisticsResponse> GetStatisticsAsync([FromBody] StatisticsRequest request)
        {
            // Mặc định lấy dữ liệu trong ngày hiện tại nếu không có tham số
            var startDate = request.startDate ??= DateTime.Today;
            var endDate = request.endDate ??= DateTime.Today.AddDays(1);
            var period = request.Period.ToLower();

            // Lấy dữ liệu từ các repository
            var revenue = await _statisticRepository.GetRevenueAsync(startDate, endDate);
            var customers = await _statisticRepository.GetCustomersAsync(startDate, endDate);
            var dishes = await _statisticRepository.GetDishesAsync(startDate, endDate);
            var reservations = await _statisticRepository.GetReservationsAsync(startDate, endDate);

            var result = new StatisticsResponse { Period = period };

            // Xử lý dữ liệu theo khoảng thời gian
            switch (period)
            {
                case "day":
                    result.Data = GetDailyData(revenue, customers, dishes, reservations);
                    break;
                case "week":
                    result.Data = GetWeeklyData(revenue, customers, dishes, reservations, startDate);
                    break;
                case "month":
                    result.Data = GetMonthlyData(revenue, customers, dishes, reservations, startDate);
                    break;
                case "quarter":
                    result.Data = GetQuarterlyData(revenue, customers, dishes, reservations, startDate);
                    break;
                case "year":
                    result.Data = GetYearlyData(revenue, customers, dishes, reservations, startDate);
                    break;
                default:
                    throw new ArgumentException("Khoảng thời gian không hợp lệ");
            }

            return result;
        }


        private List<StatisticsData> GetDailyData(
            List<TblPayment> revenue,
            List<TblOrderInfo> customers,
            List<TblOrderDetail> dishes,
            List<TblReservation> reservations)
        {
            var revenueByHour = revenue
                .GroupBy(p => p.CreatedAt.Hour)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var customersByHour = customers
                .GroupBy(o => o.CreatedAt.Hour)
                .ToDictionary(g => g.Key, g => g.Select(o => o.CusId).Distinct().Count());

            var dishesByHour = dishes
                .Join(customers,
                      d => d.OrdId,
                      o => o.OrdId,
                      (d, o) => new { Dish = d, Order = o })
                .GroupBy(x => x.Order.CreatedAt.Hour)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Dish.OdtQuantity));

            var reservationsByHour = reservations
                .GroupBy(r => r.ResDate.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            return Enumerable.Range(0, 24).Select(hour => new StatisticsData
            {
                Time = $"{hour}h",
                Revenue = revenueByHour.TryGetValue(hour, out var rev) ? rev : 0,
                Customers = customersByHour.TryGetValue(hour, out var cust) ? cust : 0,
                Dishes = dishesByHour.TryGetValue(hour, out var dish) ? dish : 0,
                Reservations = reservationsByHour.TryGetValue(hour, out var res) ? res : 0
            }).ToList();
        }

        private List<StatisticsData> GetWeeklyData(
            List<TblPayment> revenue,
            List<TblOrderInfo> customers,
            List<TblOrderDetail> dishes,
            List<TblReservation> reservations,
            DateTime startDate)
        {
            var daysInWeek = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i).DayOfWeek.ToString())
                .Distinct()
                .ToList();

            var revenueByDay = revenue
                .GroupBy(p => p.CreatedAt.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var customersByDay = customers
                .GroupBy(o => o.CreatedAt.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Select(o => o.CusId).Distinct().Count());

            var dishesByDay = dishes
                .Join(customers,
                      d => d.OrdId,
                      o => o.OrdId,
                      (d, o) => new { Dish = d, Order = o })
                .GroupBy(x => x.Order.CreatedAt.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Dish.OdtQuantity));

            var reservationsByDay = reservations
                .GroupBy(r => r.ResDate.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return daysInWeek.Select(day => new StatisticsData
            {
                Time = day,
                Revenue = revenueByDay.TryGetValue(day, out var rev) ? rev : 0,
                Customers = customersByDay.TryGetValue(day, out var cust) ? cust : 0,
                Dishes = dishesByDay.TryGetValue(day, out var dish) ? dish : 0,
                Reservations = reservationsByDay.TryGetValue(day, out var res) ? res : 0
            }).ToList();
        }

        private List<StatisticsData> GetMonthlyData(
            List<TblPayment> revenue,
            List<TblOrderInfo> customers,
            List<TblOrderDetail> dishes,
            List<TblReservation> reservations,
            DateTime startDate)
        {
            var weeksInMonth = Enumerable.Range(1, 5).Select(i => $"Week {i}").ToList();

            var revenueByWeek = revenue
                .GroupBy(p => GetWeekOfMonth(p.CreatedAt))
                .ToDictionary(g => $"Week {g.Key}", g => g.Sum(p => p.Amount));

            var customersByWeek = customers
                .GroupBy(o => GetWeekOfMonth(o.CreatedAt))
                .ToDictionary(g => $"Week {g.Key}", g => g.Select(o => o.CusId).Distinct().Count());

            var dishesByWeek = dishes
                .Join(customers,
                      d => d.OrdId,
                      o => o.OrdId,
                      (d, o) => new { Dish = d, Order = o })
                .GroupBy(x => GetWeekOfMonth(x.Order.CreatedAt))
                .ToDictionary(g => $"Week {g.Key}", g => g.Sum(x => x.Dish.OdtQuantity));

            var reservationsByWeek = reservations
                .GroupBy(r => GetWeekOfMonth(r.ResDate))
                .ToDictionary(g => $"Week {g.Key}", g => g.Count());

            return weeksInMonth.Select(week => new StatisticsData
            {
                Time = week,
                Revenue = revenueByWeek.TryGetValue(week, out var rev) ? rev : 0,
                Customers = customersByWeek.TryGetValue(week, out var cust) ? cust : 0,
                Dishes = dishesByWeek.TryGetValue(week, out var dish) ? dish : 0,
                Reservations = reservationsByWeek.TryGetValue(week, out var res) ? res : 0
            }).ToList();
        }

        private int GetWeekOfMonth(DateTime date)
        {
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var weekOfYearFirstDay = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(firstDayOfMonth, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            var weekOfYearDate = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return weekOfYearDate - weekOfYearFirstDay + 1;
        }

        private List<StatisticsData> GetQuarterlyData(
            List<TblPayment> revenue,
            List<TblOrderInfo> customers,
            List<TblOrderDetail> dishes,
            List<TblReservation> reservations,
            DateTime startDate)
        {
            var monthsInQuarter = Enumerable.Range(0, 3)
                .Select(i => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(((startDate.Month - 1) / 3 * 3 + i + 1)))
                .ToList();

            var revenueByMonth = revenue
                .GroupBy(p => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.CreatedAt.Month))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            var customersByMonth = customers
                .GroupBy(o => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(o.CreatedAt.Month))
                .ToDictionary(g => g.Key, g => g.Select(o => o.CusId).Distinct().Count());

            var dishesByMonth = dishes
                .Join(customers,
                      d => d.OrdId,
                      o => o.OrdId,
                      (d, o) => new { Dish = d, Order = o })
                .GroupBy(x => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Order.CreatedAt.Month))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Dish.OdtQuantity));

            var reservationsByMonth = reservations
                .GroupBy(r => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(r.ResDate.Month))
                .ToDictionary(g => g.Key, g => g.Count());

            return monthsInQuarter.Select(month => new StatisticsData
            {
                Time = month,
                Revenue = revenueByMonth.TryGetValue(month, out var rev) ? rev : 0,
                Customers = customersByMonth.TryGetValue(month, out var cust) ? cust : 0,
                Dishes = dishesByMonth.TryGetValue(month, out var dish) ? dish : 0,
                Reservations = reservationsByMonth.TryGetValue(month, out var res) ? res : 0
            }).ToList();
        }

        private List<StatisticsData> GetYearlyData(
            List<TblPayment> revenue,
            List<TblOrderInfo> customers,
            List<TblOrderDetail> dishes,
            List<TblReservation> reservations,
            DateTime startDate)
        {
            var quarters = new[] { "Q1", "Q2", "Q3", "Q4" };

            var revenueByQuarter = revenue
                .GroupBy(p => (p.CreatedAt.Month - 1) / 3 + 1)
                .ToDictionary(g => $"Q{g.Key}", g => g.Sum(p => p.Amount));

            var customersByQuarter = customers
                .GroupBy(o => (o.CreatedAt.Month - 1) / 3 + 1)
                .ToDictionary(g => $"Q{g.Key}", g => g.Select(o => o.CusId).Distinct().Count());

            var dishesByQuarter = dishes
                .Join(customers,
                      d => d.OrdId,
                      o => o.OrdId,
                      (d, o) => new { Dish = d, Order = o })
                .GroupBy(x => (x.Order.CreatedAt.Month - 1) / 3 + 1)
                .ToDictionary(g => $"Q{g.Key}", g => g.Sum(x => x.Dish.OdtQuantity));

            var reservationsByQuarter = reservations
                .GroupBy(r => (r.ResDate.Month - 1) / 3 + 1)
                .ToDictionary(g => $"Q{g.Key}", g => g.Count());

            return quarters.Select(quarter => new StatisticsData
            {
                Time = quarter,
                Revenue = revenueByQuarter.TryGetValue(quarter, out var rev) ? rev : 0,
                Customers = customersByQuarter.TryGetValue(quarter, out var cust) ? cust : 0,
                Dishes = dishesByQuarter.TryGetValue(quarter, out var dish) ? dish : 0,
                Reservations = reservationsByQuarter.TryGetValue(quarter, out var res) ? res : 0
            }).ToList();
        }


        public async Task<List<TopDishDto>> GetTopDishesAsync()
        {
            var currentDate = DateTime.Now;

            // Kỳ hiện tại: 30 ngày gần nhất (từ hôm nay về trước 30 ngày)
            var currentPeriodStart = currentDate.AddDays(-30);
            var currentPeriodEnd = currentDate;

            // Kỳ trước: 30 ngày trước đó (từ 60 ngày trước đến 30 ngày trước)
            var previousPeriodStart = currentDate.AddDays(-60);
            var previousPeriodEnd = currentDate.AddDays(-30);

            // Step 1: Lấy top 5 món ăn của kỳ hiện tại (30 ngày gần nhất)
            var currentPeriodData = await _context.TblOrderDetails
                .Where(od => od.CreatedAt >= currentPeriodStart && od.CreatedAt < currentPeriodEnd)
                .Join(_context.TblMenus,
                      od => od.MnuId,
                      m => m.MnuId,
                      (od, m) => new { OrderDetail = od, Menu = m })
                .GroupBy(x => new
                {
                    x.Menu.MnuId,
                    x.Menu.MnuName,
                    x.Menu.MnuPrice,
                    x.Menu.MnuImage
                })
                .Select(g => new
                {
                    MnuId = g.Key.MnuId,
                    MnuName = g.Key.MnuName,
                    MnuPrice = g.Key.MnuPrice,
                    MnuImage = g.Key.MnuImage,
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(5).ToListAsync(); 

            // Step 2: Lấy dữ liệu kỳ trước cho cùng những món ăn đó
            var dishIds = currentPeriodData.Select(x => x.MnuId).ToList();

            var previousPeriodData = await _context.TblOrderDetails
                .Where(od => od.CreatedAt >= previousPeriodStart && od.CreatedAt < previousPeriodEnd
                          && dishIds.Contains(od.MnuId))
                .GroupBy(od => od.MnuId)
                .Select(g => new
                {
                    MnuId = g.Key,
                    OrderCount = g.Count()
                })
                .ToListAsync();

            // Step 3: Kết hợp dữ liệu và tính growth percentage
            var result = currentPeriodData.Select(current =>
            {
                var previous = previousPeriodData.FirstOrDefault(p => p.MnuId == current.MnuId);
                var previousCount = previous?.OrderCount ?? 0;

                // Công thức tính growth percentage: (current - previous) / previous * 100
                var growthPercentage = previousCount == 0
                    ? (current.OrderCount > 0 ? 100m : 0m) // Edge case: kỳ trước = 0
                    : Math.Round(((decimal)(current.OrderCount - previousCount) / previousCount) * 100, 2);

                return new TopDishDto
                {
                    MnuId = current.MnuId,
                    MnuName = current.MnuName,
                    MnuPrice = current.MnuPrice,
                    MnuImage = current.MnuImage,
                    QuantitySold = current.OrderCount,
                    GrowthPercentage = growthPercentage
                };
            }).ToList();

            return result;
        }

        public async Task<TableUsageResponse> GetTableUsageReportAsync(int month, int year)
        {
            // Validate input
            if (month < 1 || month > 12)
                throw new ArgumentException("Tháng phải từ 1 đến 12");

            if (year < 1900 || year > 2100)
                throw new ArgumentException("Năm không hợp lệ");

            var startDate = new DateTime(year, month, 1);
            var currentDate = DateTime.Now.Date;

            var endDate = (year == currentDate.Year && month == currentDate.Month)
                ? currentDate.AddDays(1)
                : startDate.AddMonths(1);

            var totalDays = (endDate.AddDays(-1) - startDate).Days + 1;

            // Sử dụng repository chuyên dụng cho performance
            var tableUsageData = await _statisticRepository.GetTableUsageDataAsync(startDate, endDate);

            var totalOrders = tableUsageData.Sum(x => x.OrderCount);
            var tablesUsed = tableUsageData.Count(x => x.OrderCount > 0);

            var tableUsageList = tableUsageData.Select(data =>
            {
                var conversionRate = data.ReservationCount == 0 ? 0m :
                    Math.Round((decimal)data.OrderCount / data.ReservationCount * 100, 2);

                var usageRate = totalOrders == 0 ? 0m :
                    Math.Round((decimal)data.OrderCount / totalOrders * 100, 2);

                string status = usageRate switch
                {
                    0 => "Không được sử dụng, cần chú ý",
                    > 0 and <= 30 => "Thấp",
                    > 30 and <= 60 => "Trung bình",
                    > 60 => "Cao",
                    _ => "Không xác định"
                };

                return new TableUsageDto
                {
                    TableNumber = data.TableNumber,
                    Capacity = data.Capacity,
                    ReservationCount = data.ReservationCount,
                    OrderCount = data.OrderCount,
                    ConversionRate = conversionRate,
                    UsageRate = usageRate,
                    Status = status
                };
            }).ToList();

            var averageUsageRate = tableUsageData.Count == 0 ? 0m :
                Math.Round((decimal)tablesUsed / tableUsageData.Count * 100, 2);

            return new TableUsageResponse
            {
                Month = $"{month:D2}/{year}",
                TotalDays = totalDays,
                TotalTables = tableUsageData.Count,
                TablesUsed = tablesUsed,
                AverageUsageRate = averageUsageRate,
                TotalOrders = totalOrders,
                Tables = tableUsageList
            };
        }
    }
}
