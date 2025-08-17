using RestaurantManagement.Service.Dtos.StaffDto;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : BaseApiController
    {
        readonly IServiceProvider _serviceProvider;
        public StaffController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        // Cho phép admin xem danh sách nhân viên
        [Authorize(Policy = "AdminOrManagerPolicy")]
        [HttpPost("get-all-staff")]
        public async Task<IActionResult> GetAllStaff([FromBody] StaffModels pagingModel)
        {
            var staffService = _serviceProvider.GetRequiredService<IStaffService>();
            var staffList = await staffService.GetAllStaffAsync(pagingModel);
            var result = new PaginatedList<GetStaffByIdDto>(staffList.ToList(), staffList.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Ok(result);
        }
        // Cho phép admin xem chi tiết nhân viên
        [Authorize(Policy = "AdminOrManagerPolicy")]
        [HttpGet("get-staff-by-id/{id}")]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            var staffService = _serviceProvider.GetRequiredService<IStaffService>();
            var staff = await staffService.GetStaffByIdAsync(id);
            if (staff == null)
                throw new ErrorException(StatusCodeEnum.E01); // Nhân viên không tồn tại
            return Ok(staff);
        }
        // Chỉ admin được thêm nhân viên
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost("add-staff")]
        public async Task<IActionResult> AddStaff([FromBody] StaffDto staffDto)
        {
            if (staffDto == null)
                return BadRequest(StatusCodeEnum.BadRequest);

            var staffService = _serviceProvider.GetRequiredService<IStaffService>();
            var newStaff = await staffService.AddStaffAsync(staffDto);
            return Ok(newStaff);
        }
        // Chỉ admin được cập nhật thông tin nhân viên
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("update-staff")]
        public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateStaffProfileDto staffProfileDto)
        {
            if (staffProfileDto == null)
                return BadRequest(StatusCodeEnum.BadRequest);

            var staffService = _serviceProvider.GetRequiredService<IStaffService>();
            var updatedStaff = await staffService.UpdateStaffProfileAsync(id, staffProfileDto);
            if (updatedStaff == null)
                throw new ErrorException(StatusCodeEnum.E01);
            return Ok();
        }
        // Chỉ admin được xoá nhân viên
        [Authorize(Policy = "AdminPolicy")]
        [HttpDelete("delete-staff/{id}")]
        public async Task<IActionResult> DeleteStaff(Guid id)
        {
            var staffService = _serviceProvider.GetRequiredService<IStaffService>();
            await staffService.DeleteStaffAsync(id);
            return Ok(new {message = StatusCodeEnum.E02 }); 
        }
    }
}
