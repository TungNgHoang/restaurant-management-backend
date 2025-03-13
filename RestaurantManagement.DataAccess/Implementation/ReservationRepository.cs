using Microsoft.EntityFrameworkCore;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.DbContexts;
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
        public async Task<List<TblReservation>> GetOverlappingReservationsAsync(DateTime start, DateTime? end)
        {
            return await _context.TblReservations
                .Where(r => r.ResDate < end && r.ResEndTime > start && !r.IsDeleted)
                .ToListAsync();
        }
    }
}
