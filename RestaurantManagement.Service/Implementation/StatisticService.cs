using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class StatisticService : BaseService, IStatisticService
    {
        private readonly IStatisticRepository _statisticRepository;
        public StatisticService(AppSettings appSettings, IMapper mapper, IStatisticRepository statisticRepository) : base(appSettings, mapper)
        {
            _statisticRepository = statisticRepository;
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

        public Task<StatisticsResponse> GetStatisticsAsync(StatisticsRequest request, string userEmail, string userRole)
        {
            throw new NotImplementedException();
        }
    }
}
