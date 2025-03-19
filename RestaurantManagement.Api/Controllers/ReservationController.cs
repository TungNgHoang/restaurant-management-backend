using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Service.Interfaces;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Core.Enums;
using System.Runtime.CompilerServices;
using RestaurantManagement.Service.Dtos.ReserDto;
using RestaurantManagement.Service.ApiModels;
using Microsoft.AspNetCore.Http.HttpResults;
using RestaurantManagement.Core.ApiModels;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : BaseApiController
    {
        public IReservationService _reservationService { get; set; }

        public ReservationController(IServiceProvider serviceProvider, IReservationService reservationService) : base(serviceProvider)
        {
            _reservationService = reservationService;
        }

        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequestDto request)
        {
            try
            {
                var availableTables = await _reservationService.GetAvailableTablesAsync(request);
                return Success(new { Success = true, Data = availableTables });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("create-reservation")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var reservation = await _reservationService.CreateReservationAsync(request);
                return Success(new { Success = true, Data = reservation, Message = "Đặt bàn thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        //API get all my reservation from database
        [HttpPost("get-reservation")]
        public async Task<IActionResult> GetReservations([FromBody] ReserModel pagingModel)
        {
            var reservations = await _reservationService.GetAllReservationsAsync(pagingModel);
            var listResult = new PaginatedList<ReserDto>(reservations.ToList(), reservations.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Success(listResult);
        }
    }
}
