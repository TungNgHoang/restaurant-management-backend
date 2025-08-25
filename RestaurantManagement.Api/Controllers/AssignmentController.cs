using RestaurantManagement.Service.Dtos.AttendanceDto;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _AssignmentService;

        public AssignmentController(IAssignmentService shiftAssignmentService)
        {
            _AssignmentService = shiftAssignmentService;
        }

        [HttpPost("create-assignment")]
        public async Task<IActionResult> CreateAssignment([FromBody] AssignmentDto dto)
        {
            try
            {
                await _AssignmentService.CreateAssignmentAsync(dto);
                return Ok(new { Success = true, Message = StatusCodeEnum.G02});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("update-assignment")]
        public async Task<IActionResult> UpdateAssignment([FromBody] AssignmentDto dto)
        {
            try
            {
                await _AssignmentService.UpdateAssignmentAsync(dto);
                return Ok(new { Success = true, Message = StatusCodeEnum.G04 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("assignments-grouped")]
        public async Task<IActionResult> GetAssignmentsGrouped()
        {
            var data = await _AssignmentService.GetAssignmentsGroupedByDateAsync();
            return Ok(data);
        }

        [HttpGet("assignments-by-date")]
        public async Task<IActionResult> GetAssignmentsByDate([FromQuery] DateOnly date)
        {
            try
            {
                var assignments = await _AssignmentService.GetAssignmentsByDateAsync(date);
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
