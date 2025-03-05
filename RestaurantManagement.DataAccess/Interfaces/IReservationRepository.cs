using RestaurantManagement.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IReservationRepository
    {
        Task<List<TblReservation>> GetReservationsByTimeRange(DateTime date, TimeOnly startTime, TimeOnly endTime);
    }
}
