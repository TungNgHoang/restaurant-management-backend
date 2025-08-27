using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Service.Dtos.AttendanceDto;
using RestaurantManagement.Service.Implementation;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Giới hạn quyền truy cập cho quản lý (tùy chọn)
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeShiftService _employeeService;

        public EmployeeController(IEmployeeShiftService employeeService)
        {
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        }

        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
        {
            if (dto == null || dto.StaId == Guid.Empty || dto.AssignmentId == Guid.Empty)
            {
                return BadRequest(GetStatusCodeDescription(StatusCodeEnum.H07));
            }

            try
            {
                var result = await _employeeService.EmployeeCheckInAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var statusCode = MapExceptionToStatusCode(ex.Message);
                return BadRequest(new { StatusCode = statusCode, Message = GetStatusCodeDescription(statusCode) });
            }
        }

        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
        {
            if (dto == null || dto.StaId == Guid.Empty || dto.AssignmentId == Guid.Empty)
            {
                return BadRequest(GetStatusCodeDescription(StatusCodeEnum.H07));
            }

            try
            {
                var result = await _employeeService.EmployeeCheckOutAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var statusCode = MapExceptionToStatusCode(ex.Message);
                return BadRequest(new { StatusCode = statusCode, Message = GetStatusCodeDescription(statusCode) });
            }
        }

        // Helper method để lấy Description từ StatusCodeEnum
        private string GetStatusCodeDescription(StatusCodeEnum statusCode)
        {
            var fieldInfo = statusCode.GetType().GetField(statusCode.ToString());
            var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return descriptionAttribute != null ? descriptionAttribute.Description : statusCode.ToString();
        }

        // Helper method để ánh xạ thông điệp lỗi sang StatusCodeEnum
        private StatusCodeEnum MapExceptionToStatusCode(string message)
        {
            return message switch
            {
                "Chỉ có thể check-in khi ca đang diễn ra." => StatusCodeEnum.H01,
                "Nhân viên đã check-in trước đó." => StatusCodeEnum.H02,
                "Chỉ có thể check-out khi ca đang diễn ra." => StatusCodeEnum.H03,
                "Chưa check-in." => StatusCodeEnum.H04,
                "Đã check-out trước đó." => StatusCodeEnum.H05,
                "Phân công ca làm không tồn tại." => StatusCodeEnum.H06,
                _ => StatusCodeEnum.Error // Mặc định nếu không khớp
            };
        }
    }
}