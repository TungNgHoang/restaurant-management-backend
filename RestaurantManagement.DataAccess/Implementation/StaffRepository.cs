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
    public class StaffRepository : Repository<TblStaff>, IStaffRepository
    {
        private readonly RestaurantDBContext _context;

        public StaffRepository(RestaurantDBContext context) : base(context)
        {
            _context = context;
        }

        // Implement any specific methods for staff repository here
    }
}
