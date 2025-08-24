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
                return Success(new { Success = true, Data = availableTables });
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
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [Authorize(Policy = "MCPolicy")]
        [HttpPost("get-reservation")]
        public async Task<IActionResult> GetReservations([FromBody] ReserModel pagingModel)
        {
            var reservations = await _reservationService.GetAllReservationsAsync(pagingModel);
            var listResult = new PaginatedList<ReserDto>(reservations.ToList(), reservations.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Success(listResult);
        }

        [Authorize(Policy = "MCPolicy")]
        [HttpPut("{resId}/check-in")]
        public async Task<IActionResult> CheckInReservation(Guid resId, int actualNumber)
        {
            try
            {
                await _reservationService.CheckInReservationAsync(resId, actualNumber);
                return Success();
            }
            catch
            {
                throw new ErrorException(StatusCodeEnum.Error);
            }
        }

        [Authorize(Policy = "MCPolicy")]
        [HttpGet("{resId}")]
        public async Task<IActionResult> GetReservationById(Guid resId)
        {
            try
            {
                var reservation = await _reservationService.GetReservationByIdAsync(resId);
                return Success(reservation);
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodeEnum.Error, ex.Message);
            }
        }

        [Authorize(Policy = "MCPolicy")]
        [HttpPost("{resId}/cancel-reservation")]
        public async Task<IActionResult> CancelReservation(Guid resId)
        {
            await _reservationService.CancelReservationAsync(resId);
            return Success();
        }

        [Authorize(Policy = "BillingPolicy")]
        [HttpPut("update-reservation/{id}")]
        public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] UpdateReservationRequestDto reserDto)
        {
            try
            {
                var updatedReservation = await _reservationService.UpdateReservationAsync(id, reserDto);
                if (updatedReservation == null) return NotFound();
                return Success(updatedReservation);
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodeEnum.Error, ex.Message); //trycatch để bắt lỗi và trả về thông báo lỗi
            }
        }
    }
}