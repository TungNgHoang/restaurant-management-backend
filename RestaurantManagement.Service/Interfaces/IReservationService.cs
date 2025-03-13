using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos;
using RestaurantManagement.Service.Dtos.ReserDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IReservationService
    {
        Task<List<AvailableTableDto>> GetAvailableTablesAsync(CheckAvailabilityRequestDto request);
        Task<ReservationResponseDto> CreateReservationAsync(CreateReservationRequestDto request);
    }
}
