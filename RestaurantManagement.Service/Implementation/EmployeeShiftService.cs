using RestaurantManagement.Service.Dtos.AttendanceDto;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.DataAccess.Implementation;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Core.Enums;
using System;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class EmployeeShiftService : BaseService, IEmployeeShiftService
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<TblShiftAssignment> _assignmentRepository;
        private readonly IRepository<TblShift> _shiftRepository;
        private readonly IRepository<TblAttendance> _attendanceRepository;
        private readonly RestaurantDBContext _dbContext;

        public EmployeeShiftService(
            AppSettings appSettings,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IRepository<TblShiftAssignment> assignmentRepository,
            IRepository<TblShift> shiftRepository,
            IRepository<TblAttendance> attendanceRepository,
            RestaurantDBContext dbContext)
            : base(appSettings, mapper, httpContextAccessor, dbContext)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _assignmentRepository = assignmentRepository;
            _shiftRepository = shiftRepository;
            _attendanceRepository = attendanceRepository;
            _dbContext = dbContext;
        }

        // Phương thức Check-in
        public async Task<AttendanceResultDto> EmployeeCheckInAsync(CheckInDto dto)
        {
            try
            {
                // Kiểm tra phân công ca làm
                var assignment = await _assignmentRepository.FindByIdAsync(dto.AssignmentId);
                if (assignment == null || assignment.StaId != dto.StaId)
                {
                    throw new Exception(StatusCodeEnum.H06.ToString()); // Phân công ca làm không tồn tại
                }

                // Lấy thông tin ca làm
                var shift = await _shiftRepository.FindByIdAsync(assignment.ShiftId);
                if (shift == null)
                {
                    throw new Exception(StatusCodeEnum.H06.ToString()); // Phân công ca làm không tồn tại
                }

                // Tính thời gian bắt đầu ca
                var workDate = assignment.WorkDate.ToDateTime(new TimeOnly());
                var startTime = workDate + shift.StartTime.ToTimeSpan();
                var endTime = workDate + shift.EndTime.ToTimeSpan();
                var checkInTime = ToGmt7(dto.CheckInTime); // Chuyển sang múi giờ +07

                // Debug thông tin thời gian
                Console.WriteLine($"Check-in Time: {checkInTime}, Start Time: {startTime}, End Time: {endTime}");

                // Kiểm tra ca đang diễn ra
                if (checkInTime < startTime || checkInTime > endTime)
                {
                    throw new Exception(StatusCodeEnum.H01.ToString()); // Chỉ có thể check-in khi ca đang diễn ra
                }

                // Kiểm tra attendance đã tồn tại chưa
                var attendances = await _attendanceRepository.FilterAsync(a => a.AssignmentId == dto.AssignmentId && a.StaId == dto.StaId);
                var attendance = attendances.FirstOrDefault();

                if (attendance != null && attendance.CheckIn.HasValue)
                {
                    throw new Exception(StatusCodeEnum.H02.ToString()); // Nhân viên đã check-in trước đó
                }

                // Tạo hoặc cập nhật attendance
                if (attendance == null)
                {
                    attendance = new TblAttendance
                    {
                        AttendanceId = Guid.NewGuid(),
                        StaId = dto.StaId,
                        AssignmentId = dto.AssignmentId,
                        CheckIn = checkInTime,
                        Status = CalculateCheckInStatus(startTime.TimeOfDay, checkInTime.TimeOfDay)
                    };
                    await _attendanceRepository.InsertAsync(attendance);
                }
                else
                {
                    attendance.CheckIn = checkInTime;
                    attendance.Status = CalculateCheckInStatus(startTime.TimeOfDay, checkInTime.TimeOfDay);
                    await _attendanceRepository.UpdateAsync(attendance);
                }

                return MapToResultDto(attendance);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"Error in CheckIn: {ex.Message}, StackTrace: {ex.StackTrace}");
                throw; // Ném lại ngoại lệ để controller xử lý
            }
        }

        // Phương thức Check-out
        public async Task<AttendanceResultDto> EmployeeCheckOutAsync(CheckOutDto dto)
        {
            // Kiểm tra phân công ca làm
            var assignment = await _assignmentRepository.FindByIdAsync(dto.AssignmentId);
            if (assignment == null || assignment.StaId != dto.StaId)
            {
                throw new Exception(StatusCodeEnum.H06.ToString()); // Phân công ca làm không tồn tại
            }

            // Lấy thông tin ca làm
            var shift = await _shiftRepository.FindByIdAsync(assignment.ShiftId);
            if (shift == null)
            {
                throw new Exception(StatusCodeEnum.H06.ToString()); // Phân công ca làm không tồn tại
            }

            // Tính thời gian kết thúc ca
            var workDate = assignment.WorkDate.ToDateTime(new TimeOnly());
            var assignmentTime = workDate + shift.EndTime.ToTimeSpan(); // Lấy thời gian kết thúc ca
            var checkOutTime = ToGmt7(dto.CheckOutTime); // Chuyển sang múi giờ +07

            // Kiểm tra ca đang diễn ra
            if (checkOutTime < workDate + shift.StartTime.ToTimeSpan() || checkOutTime > assignmentTime)
            {
                throw new Exception(StatusCodeEnum.H03.ToString()); // Chỉ có thể check-out khi ca đang diễn ra
            }

            // Lấy attendance dựa trên StaId và AssignmentId
            var attendances = await _attendanceRepository.FilterAsync(a => a.AssignmentId == dto.AssignmentId && a.StaId == dto.StaId);
            var attendance = attendances.FirstOrDefault();

            if (attendance == null)
            {
                throw new Exception(StatusCodeEnum.H04.ToString()); // Chưa check-in hoặc bản ghi không tồn tại
            }

            if (!attendance.CheckIn.HasValue)
            {
                throw new Exception(StatusCodeEnum.H04.ToString()); // Chưa check-in
            }

            if (attendance.CheckOut.HasValue)
            {
                throw new Exception(StatusCodeEnum.H05.ToString()); // Đã check-out trước đó
            }

            // So sánh checkoutTime với assignmentTime để cập nhật Status
            attendance.Status = CalculateCheckOutStatus(attendance.Status, assignmentTime.TimeOfDay, checkOutTime.TimeOfDay);

            // Cập nhật Check-out time
            attendance.CheckOut = checkOutTime;

            // Tính tổng giờ làm (giờ làm thực tế từ CheckIn đến CheckOut)
            var totalHours = (checkOutTime - attendance.CheckIn.Value).TotalHours;

            // Cập nhật tblPayroll
            var payroll = await _dbContext.TblPayrolls
                .FirstOrDefaultAsync(p => p.StaId == dto.StaId);
            if (payroll == null)
            {
                payroll = new TblPayroll { StaId = dto.StaId };
            }
            payroll.TotalHours = (decimal)totalHours;

            // Lấy BaseSalary từ TblStaff và tính TotalSalary
            var staff = await _dbContext.TblStaffs.FindAsync(dto.StaId);
            if (staff != null && staff.StaBaseSalary.HasValue && staff.StaBaseSalary > 0)
            {
                payroll.TotalSalary = (staff.StaBaseSalary.Value / 160m) * (decimal)totalHours;
            }

            // Lưu hoặc cập nhật payroll
            if (payroll.PayrollId == default)
            {
                await _dbContext.TblPayrolls.AddAsync(payroll);
            }
            else
            {
                _dbContext.TblPayrolls.Update(payroll);
            }

            // Cập nhật attendance
            await _attendanceRepository.UpdateAsync(attendance);
            await _dbContext.SaveChangesAsync();

            return MapToResultDto(attendance);
        }

        // Tính trạng thái cho Check-in
        private string CalculateCheckInStatus(TimeSpan assignmentTime, TimeSpan checkInTime)
        {
            var gracePeriod = TimeSpan.FromMinutes(10); // Khoảng cách cho phép
            if (checkInTime < assignmentTime)
                return "Early";
            else if (checkInTime <= assignmentTime + gracePeriod)
                return "OnTime";
            else
                return "Late";
        }

        // Tính trạng thái cho Check-out
        private string CalculateCheckOutStatus(string currentStatus, TimeSpan assignmentTime, TimeSpan checkOutTime)
        {
            var gracePeriod = TimeSpan.FromMinutes(10);
            if (checkOutTime < assignmentTime - gracePeriod)
                return "Early";
            else if (checkOutTime <= assignmentTime + gracePeriod)
                return currentStatus;
            else if (currentStatus == "Late")
                return "LateandEarly";
            else
                return currentStatus;
        }

        // Map entity sang DTO
        private AttendanceResultDto MapToResultDto(TblAttendance attendance)
        {
            return new AttendanceResultDto
            {
                AttendanceId = attendance.AttendanceId,
                StaId = attendance.StaId,
                AssignmentId = attendance.AssignmentId,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                Status = attendance.Status
            };
        }

        // Helper method từ BaseService
        private DateTime ToGmt7(DateTime utcTime)
        {
            return utcTime.ToUniversalTime().AddHours(7); // Điều chỉnh sang múi giờ +07
        }
    }
}