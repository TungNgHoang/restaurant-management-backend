using RestaurantManagement.DataAccess.Implementation;
using RestaurantManagement.Service.Dtos.AttendanceDto;

namespace RestaurantManagement.Service.Implementation
{
    public class AssignmentService : BaseService, IAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<TblShiftAssignment> _assignmentRepository;
        private readonly IRepository<TblShift> _shiftRepository;
        private readonly IRepository<TblStaff> _staffRepository;
        private readonly RestaurantDBContext _dbContext;

        public AssignmentService(
            AppSettings appSettings,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IRepository<TblShiftAssignment> assignmentRepository,
            IRepository<TblShift> shiftRepository,
            IRepository<TblStaff> staffRepository,
            RestaurantDBContext dbContext
            ) : base(appSettings, mapper, httpContextAccessor, dbContext)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _assignmentRepository = assignmentRepository;
            _shiftRepository = shiftRepository;
            _staffRepository = staffRepository;
            _dbContext = dbContext;
        }

        // Thêm mới và cập nhật phân công ca làm việc
        public async Task SaveAssignmentsAsync(AssignmentRequestDto dto)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())

                try
                {
                    var currentTime = ToGmt7(DateTime.UtcNow);

                    // Gom nhóm theo (ShiftTime, ShiftDate)
                    var groupedAssignments = dto.Assignments
                        .GroupBy(a => new { a.ShiftTime, a.ShiftDate })
                        .ToList();

                    foreach (var group in groupedAssignments)
                    {
                        var shift = await GetShiftByTimeAsync(group.Key.ShiftTime);

                        // Check danh sách nhân viên trong nhóm (gom query 1 lần)
                        var staffs = await GetValidStaffsAsync(group.Select(x => x.StaId).ToList());

                        // Lấy assignment đang có trong DB
                        var existingAssignments = await _assignmentRepository.FindListAsync(
                            a => a.WorkDate == group.Key.ShiftDate && a.ShiftId == shift.ShiftId
                        );

                        var existingStaIds = existingAssignments.Select(a => a.StaId).ToHashSet();
                        var newStaIds = group.Select(x => x.StaId).ToHashSet();

                        var toAdd = newStaIds.Except(existingStaIds);
                        var toRemove = existingStaIds.Except(newStaIds);

                        // Thêm mới
                        foreach (var id in toAdd)
                        {
                            var assignment = new TblShiftAssignment
                            {
                                AssignmentId = Guid.NewGuid(),
                                ShiftId = shift.ShiftId,
                                StaId = id,
                                WorkDate = group.Key.ShiftDate,
                                CreatedAt = currentTime
                            };

                            await _assignmentRepository.InsertAsync(assignment);
                        }

                        // Xoá bớt
                        foreach (var id in toRemove)
                        {
                            var assignment = existingAssignments.FirstOrDefault(a => a.StaId == id);
                            if (assignment != null)
                            {
                                await _assignmentRepository.DeleteAsync(assignment);
                            }
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
        }

        //Lấy Tất cả phân công ca làm việc là gộp theo ngày
        public async Task<List<AssignmentGroupDto>> GetAssignmentsGroupedByDateAsync()
        {
            // Lấy toàn bộ assignment
            var assignments = await _assignmentRepository.GetListAsync();

            // Gộp theo WorkDate
            var result = assignments
                .GroupBy(a => a.WorkDate)
                .Select(g => new AssignmentGroupDto
                {
                    WorkDate = g.Key,
                    Assignments = g.Select(a => new AssignmentDetailDto
                    {
                        AssignmentId = a.AssignmentId,
                        ShiftId = a.ShiftId,
                        StaId = a.StaId,
                        CreatedAt = (DateTime)a.CreatedAt
                    }).ToList()
                })
                .OrderBy(g => g.WorkDate) // sắp xếp theo ngày
                .ToList();

            return result;
        }

        public async Task<AssignmentGroupDto?> GetAssignmentsByDateAsync(DateOnly workDate)
        {
            var assignments = await _assignmentRepository.FilterAsync(a => a.WorkDate == workDate);
            var staffs = await _staffRepository.GetListAsync();
            

            if (assignments == null || !assignments.Any())
                return null;

            var result = new AssignmentGroupDto
            {
                WorkDate = workDate,
                Assignments = assignments.Select(a => new AssignmentDetailDto
                {
                    AssignmentId = a.AssignmentId,
                    ShiftId = a.ShiftId,
                    StaId = a.StaId,
                    staName = staffs.FirstOrDefault(s => s.StaId == a.StaId)?.StaName ?? string.Empty,
                    staRole = staffs.FirstOrDefault(s => s.StaId == a.StaId)?.StaRole ?? string.Empty,
                    CreatedAt = (DateTime)a.CreatedAt
                }).ToList()
            };

            return result;
        }

        // Lấy ca theo shiftTime
        private async Task<TblShift> GetShiftByTimeAsync(TimeOnly shiftTime)
        {
            var shift = (await _shiftRepository.FilterAsync(
                s => s.StartTime <= shiftTime && s.EndTime >= shiftTime
            )).FirstOrDefault();

            if (shift == null)
            {
                throw new Exception(StatusCodeEnum.G01.ToString());
            }

            return shift;
        }

        // Lấy danh sách nhân viên hợp lệ (gom query 1 lần)
        private async Task<List<TblStaff>> GetValidStaffsAsync(List<Guid> staffIds)
        {
            var staffs = (await _staffRepository.FilterAsync(s => staffIds.Contains(s.StaId))).ToList();

            if (staffs.Count != staffIds.Count)
            {
                // Có ít hơn, tức là có nhân viên không tồn tại
                throw new Exception(StatusCodeEnum.G03.ToString());
            }

            return staffs;
        }
    }
}
