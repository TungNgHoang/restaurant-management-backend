using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Models;
using RestaurantManagement.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Implementation
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly RestaurantDBContext _context; // Giả sử bạn sử dụng Entity Framework Core

        // Constructor nhận DbContext qua dependency injection
        public ReservationRepository(RestaurantDBContext context)
        {
            _context = context;
        }
        public async Task<List<TblReservation>> GetReservationsByTimeRange(DateTime date, TimeOnly startTime, TimeOnly endTime)
        {
            var reservations = await _context.TblReservations
                .Where(r => r.ResDate.Date == date.Date // Lọc theo ngày
                    && r.ResTime >= startTime           // Thời gian bắt đầu
                    && r.ResTime < endTime              // Thời gian kết thúc
                    && !r.IsDeleted)                    // Chỉ lấy reservation chưa bị xóa
                .ToListAsync();

            return reservations.ToList();
        }
    }
}
