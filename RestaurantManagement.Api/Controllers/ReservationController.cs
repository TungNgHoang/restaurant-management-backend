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
using Microsoft.AspNetCore.Authorization;

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

        [Authorize(Policy = "PublicAccess")]
        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityRequestDto request)
        {
            try
            {
                var availableTables = await _reservationService.GetAvailableTablesAsync(request);
                return Ok(new { Success = true, Data = availableTables });
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodeEnum.Error, ex.Message);
            }
        }

        [Authorize(Policy = "PublicAccess")]
        [HttpPost("create-reservation")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var reservation = await _reservationService.CreateReservationAsync(request);
                return Success(new { Success = true, Data = reservation, StatusCodeEnum.A06});
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodeEnum.Error, ex.Message);
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("get-reservation")]
        public async Task<IActionResult> GetReservations([FromBody] ReserModel pagingModel)
        {
            var reservations = await _reservationService.GetAllReservationsAsync(pagingModel);
            var listResult = new PaginatedList<ReserDto>(reservations.ToList(), reservations.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Success(listResult);
        }

        [Authorize(Policy = "StaffPolicy")]
        [HttpPut("{resId}/check-in")]
        public async Task<IActionResult> CheckInReservation(Guid resId, int actualNumber)
        {
            try
            {
                await _reservationService.CheckInReservationAsync(resId, actualNumber);
                return Ok();
            }
            catch
            {
                throw new ErrorException(StatusCodeEnum.Error);
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpGet("{resId}")]
        public async Task<IActionResult> GetReservationById(Guid resId)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByIdAsync(resId);
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodeEnum.Error, ex.Message);
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("{resId}/cancel-reservation")]
        public async Task<IActionResult> CancelReservation(Guid resId)
        {
            await _reservationService.CancelReservationAsync(resId);
            return Success();
        }
    }
}