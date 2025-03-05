using RestaurantManagement.Api.Models;
using RestaurantManagement.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservationAsync(ReservationDto reservationDto);
        
        //Task<ReservationDto> GetReservationByIdAsync(Guid id);
        // Thêm các phương thức khác vào đây
    }
}
