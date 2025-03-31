using RestaurantManagement.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IStatisticRepository
    {
        Task<List<TblPayment>> GetRevenueAsync(DateTime startDate, DateTime endDate);
        Task<List<TblOrderInfo>> GetCustomersAsync(DateTime startDate, DateTime endDate);
        Task<List<TblOrderDetail>> GetDishesAsync(DateTime startDate, DateTime endDate);
        Task<List<TblReservation>> GetReservationsAsync(DateTime startDate, DateTime endDate);
    }
}
