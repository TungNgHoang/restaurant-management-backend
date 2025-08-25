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

        // thêm mới phân công ca làm việc với shiftTime và danh sách staId
        public async Task CreateAssignmentAsync(AssignmentDto dto)
        {

            var shift = (await _shiftRepository.FilterAsync(
                s => s.StartTime <= dto.shiftTime
                  && s.EndTime >= dto.shiftTime
            )).FirstOrDefault();
            if (shift == null)
            {
                throw new Exception(StatusCodeEnum.G01.ToString());
            }
            //Kiểm tra xem staId có trong bảng TblStaff không
            foreach (var id in dto.staId)
            {
                var staff = await _staffRepository.FindByIdAsync(id);
                if (staff == null)
                {
                    throw new Exception(StatusCodeEnum.G03.ToString());
                }
            }

            var currentTime = ToGmt7(DateTime.UtcNow);
            foreach (var id in dto.staId)
            {
                var assignment = new TblShiftAssignment
                {
                    AssignmentId = Guid.NewGuid(),
                    ShiftId = shift.ShiftId,
                    StaId = id,
                    WorkDate = dto.shiftDate,
                    CreatedAt = currentTime
                };
                await _assignmentRepository.InsertAsync(assignment);
            }
            return;
        }

        // Cập nhật phân công ca làm việc
        public async Task UpdateAssignmentAsync(AssignmentDto dto)
        {
            var shift = (await _shiftRepository.FilterAsync(
                s => s.StartTime <= dto.shiftTime
                  && s.EndTime >= dto.shiftTime
            )).FirstOrDefault();
            if (shift == null)
            {
                throw new Exception(StatusCodeEnum.G01.ToString());
            }
            //Kiểm tra xem staId có trong bảng TblStaff không
            foreach (var id in dto.staId)
            {
                var staff = await _staffRepository.FindByIdAsync(id);
                if (staff == null)
                {
                    throw new Exception(StatusCodeEnum.G03.ToString());
                }
            }
            var existingAssignments = await _assignmentRepository.FindListAsync(a => a.WorkDate == dto.shiftDate && a.ShiftId == shift.ShiftId);
            var existingStaIds = existingAssignments.Select(a => a.StaId).ToHashSet();
            var newStaIds = dto.staId.ToHashSet();
            var toAdd = newStaIds.Except(existingStaIds);
            var toRemove = existingStaIds.Except(newStaIds);
            var currentTime = ToGmt7(DateTime.UtcNow);
            foreach (var id in toAdd)
            {
                var assignment = new TblShiftAssignment
                {
                    AssignmentId = Guid.NewGuid(),
                    ShiftId = shift.ShiftId,
                    StaId = id,
                    WorkDate = dto.shiftDate,
                    CreatedAt = currentTime
                };
                await _assignmentRepository.InsertAsync(assignment);
            }
            foreach (var id in toRemove)
            {
                var assignment = existingAssignments.FirstOrDefault(a => a.StaId == id);
                if (assignment != null)
                {
                    await _assignmentRepository.DeleteAsync(assignment);
                }
            }
            return;
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
                    CreatedAt = (DateTime)a.CreatedAt
                }).ToList()
            };

            return result;
        }

    }
}
