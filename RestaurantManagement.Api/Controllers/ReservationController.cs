using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.Dtos;
using RestaurantManagement.Service.Interfaces;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Core.Enums;
using System.Runtime.CompilerServices;

namespace RestaurantManagement.Api.Controllers
{
    [Route("aa")]
    [ApiController]
    public class ReservationController : BaseApiController
    {
        public IReservationService _reservationService {  get; set; }

        public ReservationController(IServiceProvider serviceProvider, IReservationService reservationService) : base(serviceProvider)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdReservation = await _reservationService.CreateReservationAsync(reservationDto);
                return Success(createdReservation);
            }
            catch
            {
                // Ghi log lỗi ở đây
                throw new ErrorException(StatusCodeEnum.Error);
            }
        }

      
    }
}
