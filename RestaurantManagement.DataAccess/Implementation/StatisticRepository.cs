using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Implementation
{
    public class StatisticRepository : IStatisticRepository
    {
        protected readonly RestaurantDBContext _context;

        public StatisticRepository(RestaurantDBContext context)
        {
            _context = context;
        }

        public async Task<List<TblOrderInfo>> GetCustomersAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TblOrderInfos
            .Where(o => !o.IsDeleted && o.CreatedAt >= startDate && o.CreatedAt < endDate)
                .ToListAsync();
        }

        public async Task<List<TblOrderDetail>> GetDishesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TblOrderDetails
                .Join(_context.TblOrderInfos,
                      od => od.OrdId,
                      oi => oi.OrdId,
                      (od, oi) => new { OrderDetail = od, OrderInfo = oi })
                .Where(x => !x.OrderDetail.IsDeleted && !x.OrderInfo.IsDeleted && x.OrderInfo.CreatedAt >= startDate && x.OrderInfo.CreatedAt < endDate)
                .Select(x => x.OrderDetail)
                .ToListAsync();
        }

        public async Task<List<TblReservation>> GetReservationsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TblReservations
                .Where(r => r.ResStatus != ReservationStatus.Canceled.ToString() && !r.IsDeleted && r.ResDate >= startDate && r.ResDate < endDate)
                .ToListAsync();
        }

        public async Task<List<TblPayment>> GetRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TblPayments
                .Where(p => !p.IsDeleted && p.CreatedAt >= startDate && p.CreatedAt < endDate)
                .ToListAsync();
        }
    }
}
