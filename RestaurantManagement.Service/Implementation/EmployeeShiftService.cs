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

        #region Check-In
        public async Task<AttendanceResultDto> EmployeeCheckInAsync(CheckInDto dto)
        {
            try
            {
                // 1. Kiểm tra phân công ca làm
                var assignment = await _assignmentRepository.FindByIdAsync(dto.AssignmentId);
                if (assignment == null || assignment.StaId != dto.StaId)
                    throw new Exception(StatusCodeEnum.H06.ToString());

                var shift = await _shiftRepository.FindByIdAsync(assignment.ShiftId);
                if (shift == null)
                    throw new Exception(StatusCodeEnum.H06.ToString());

                // 2. Tính giờ ca làm
                var workDate = assignment.WorkDate.ToDateTime(new TimeOnly());
                var startTime = workDate + shift.StartTime.ToTimeSpan();
                var endTime = workDate + shift.EndTime.ToTimeSpan();
                var checkInTime = ToGmt7(dto.CheckInTime);

                // 3. Kiểm tra hợp lệ
                if (checkInTime < startTime || checkInTime > endTime)
                    throw new Exception(StatusCodeEnum.H01.ToString());

                // 4. Attendance
                var attendance = (await _attendanceRepository.FilterAsync(a =>
                    a.AssignmentId == dto.AssignmentId && a.StaId == dto.StaId)).FirstOrDefault();

                if (attendance != null && attendance.CheckIn.HasValue)
                    throw new Exception(StatusCodeEnum.H02.ToString());

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
                Console.WriteLine($"[CheckIn] Error: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Check-Out
        public async Task<AttendanceResultDto> EmployeeCheckOutAsync(CheckOutDto dto)
        {
            try
            {
                // 1. Kiểm tra phân công
                var assignment = await _assignmentRepository.FindByIdAsync(dto.AssignmentId);
                if (assignment == null || assignment.StaId != dto.StaId)
                    throw new Exception(StatusCodeEnum.H06.ToString());

                var shift = await _shiftRepository.FindByIdAsync(assignment.ShiftId);
                if (shift == null)
                    throw new Exception(StatusCodeEnum.H06.ToString());

                // 2. Giờ ca làm
                var workDate = assignment.WorkDate.ToDateTime(new TimeOnly());
                var startTime = workDate + shift.StartTime.ToTimeSpan();
                var endTime = workDate + shift.EndTime.ToTimeSpan();
                var checkOutTime = ToGmt7(dto.CheckOutTime);

                if (checkOutTime < startTime || checkOutTime > endTime.AddHours(1))
                    throw new Exception(StatusCodeEnum.H03.ToString());

                // 3. Attendance
                var attendance = (await _attendanceRepository.FilterAsync(a =>
                    a.AssignmentId == dto.AssignmentId && a.StaId == dto.StaId)).FirstOrDefault();

                if (attendance == null || !attendance.CheckIn.HasValue)
                    throw new Exception(StatusCodeEnum.H04.ToString());

                if (attendance.CheckOut.HasValue)
                    throw new Exception(StatusCodeEnum.H05.ToString());

                attendance.CheckOut = checkOutTime;
                attendance.Status = CalculateCheckOutStatus(
                    attendance.Status, endTime.TimeOfDay, checkOutTime.TimeOfDay);

                // 4. Payroll
                var totalHours = (checkOutTime - attendance.CheckIn.Value).TotalHours;
                if (totalHours < 0) totalHours = 0;

                var month = workDate.Month;
                var year = workDate.Year;

                var payroll = await _dbContext.TblPayrolls
                    .FirstOrDefaultAsync(p => p.StaId == dto.StaId && p.Month == month && p.Year == year);

                if (payroll == null)
                {
                    payroll = new TblPayroll
                    {
                        PayrollId = Guid.NewGuid(),
                        StaId = dto.StaId,
                        Month = month,
                        Year = year,
                        TotalHours = 0,
                        TotalSalary = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _dbContext.TblPayrolls.AddAsync(payroll);
                }

                payroll.TotalHours += (decimal)totalHours;

                var staff = await _dbContext.TblStaffs.FindAsync(dto.StaId);
                if (staff != null && staff.StaBaseSalary.HasValue && staff.StaBaseSalary > 0)
                {
                    payroll.TotalSalary = (staff.StaBaseSalary.Value / 160m) * payroll.TotalHours;
                }

                await _attendanceRepository.UpdateAsync(attendance);
                await _dbContext.SaveChangesAsync();

                return MapToResultDto(attendance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CheckOut] Error: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Query Attendance
        public async Task<IEnumerable<AttendanceResultDto>> GetAttendancesByWorkDateAsync(DateTime workDate)
        {
            var dateOnly = DateOnly.FromDateTime(workDate);

            var query = from sa in _dbContext.TblShiftAssignments
                        join att in _dbContext.TblAttendances
                            on sa.AssignmentId equals att.AssignmentId into attJoin
                        from att in attJoin.DefaultIfEmpty()
                        where sa.WorkDate == dateOnly
                        select new AttendanceResultDto
                        {
                            AttendanceId = att != null ? att.AttendanceId : Guid.Empty,
                            StaId = sa.StaId,
                            AssignmentId = sa.AssignmentId,
                            ShiftId = sa.ShiftId,
                            CheckIn = att != null ? att.CheckIn : null,
                            CheckOut = att != null ? att.CheckOut : null,
                            Status = att != null ? att.Status : "Chưa check-in"
                        };

            return await query.ToListAsync();
        }

        #endregion

        #region Helpers
        private string CalculateCheckInStatus(TimeSpan start, TimeSpan checkIn)
        {
            var grace = TimeSpan.FromMinutes(10);
            if (checkIn < start) return "Early";
            if (checkIn <= start + grace) return "OnTime";
            return "Late";
        }

        private string CalculateCheckOutStatus(string current, TimeSpan end, TimeSpan checkOut)
        {
            var grace = TimeSpan.FromMinutes(10);
            if (checkOut < end - grace) return "Early";
            if (checkOut <= end + grace) return current;
            if (current == "Late") return "LateAndEarly";
            return current;
        }

        private AttendanceResultDto MapToResultDto(TblAttendance att)
        {
            return new AttendanceResultDto
            {
                AttendanceId = att.AttendanceId,
                StaId = att.StaId,
                AssignmentId = att.AssignmentId,
                CheckIn = att.CheckIn,
                CheckOut = att.CheckOut,
                Status = att.Status
            };
        }

        private DateTime ToGmt7(DateTime utcTime)
            => utcTime.ToUniversalTime().AddHours(7);
        #endregion
    }
}