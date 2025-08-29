using RestaurantManagement.DataAccess.Dtos.StaffReportDto;
using RestaurantManagement.Service.Dtos.StaffDto;
using System.Text.RegularExpressions;

namespace RestaurantManagement.Service.Implementation
{
    public class StaffService : BaseService, IStaffService
    {
        public readonly IStaffRepository _staffRepository;
        public readonly IRepository<TblUserAccount> _userAccountRepository;
        public readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StaffService(
            AppSettings appSettings, 
            IStaffRepository staffRepository, 
            IRepository<TblUserAccount> userAccountRepository, 
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor, 
            RestaurantDBContext dbContext) : base(appSettings, mapper, httpContextAccessor, dbContext)
        {
            _staffRepository = staffRepository;
            _mapper = mapper;
            _userAccountRepository = userAccountRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<GetStaffByIdDto>> GetAllStaffAsync(StaffModels pagingModel)
        {
            // Validate PageIndex and PageSize
            ValidatePagingModel(pagingModel);

            var staffList = await _staffRepository.AsNoTrackingAsync();
            var accountList = await _userAccountRepository.AsNoTrackingAsync();
            var data = from s in staffList
                       join u in accountList on s.UacId equals u.UacId
                       select new GetStaffByIdDto
                       {
                           StaId = s.StaId,
                           UacId = s.UacId,
                           StaName = s.StaName,
                           StaRole = s.StaRole,
                           StaPhone = s.StaPhone,
                           StaBaseSalary = (decimal)s.StaBaseSalary,
                           StaEmail = u.UacEmail
                       };

            var staffDtos = _mapper.Map<List<GetStaffByIdDto>>(data);
            var result = AdvancedFilter(staffDtos.AsEnumerable(), pagingModel, nameof(GetStaffByIdDto.StaName));

            return result;
        }
        private void ValidatePagingModel(StaffModels pagingModel)
        {
            if (pagingModel.PageIndex < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageIndexInvalid);
            if (pagingModel.PageSize < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageSizeInvalid);
        }

        //Get nhân viên bằng ID
        public async Task<GetStaffByIdDto> GetStaffByIdAsync(Guid id)
        {
            var staff = await _staffRepository.FindByIdAsync(id);
            if (staff == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E01);

            var userAccount = await _userAccountRepository.FindByIdAsync(staff.UacId);
            if (userAccount == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E03);

            var result = new GetStaffByIdDto
            {
                StaId = staff.StaId,
                UacId = staff.UacId,
                StaName = staff.StaName,
                StaPhone = staff.StaPhone,
                StaBaseSalary = (decimal)staff.StaBaseSalary,
                StaRole = staff.StaRole,
                StaEmail = userAccount.UacEmail
            };

            return result;
        }

        //Thêm nhân viên
        public async Task<StaffDto> AddStaffAsync(StaffDto staffDto)
        {
            var currentUserId = GetCurrentUserId();
            var currentTime = ToGmt7(DateTime.UtcNow);
            if (staffDto.StaBaseSalary <= 0)
                throw new ErrorException(StatusCodeEnum.E04);
            if (string.IsNullOrWhiteSpace(staffDto.StaPhone) || !Regex.IsMatch(staffDto.StaPhone, @"^\d{10}$"))
                throw new ErrorException(StatusCodeEnum.E05);
            // Kiểm tra email đã tồn tại chưa
            var existingAccount = await _userAccountRepository
                .FindAsync(u => u.UacEmail == staffDto.StaEmail);
            if (existingAccount != null)
                throw new ErrorException(StatusCodeEnum.E08); // tạo thêm mã lỗi cho "Email đã tồn tại"
            //Cập nhật thông tin ở bảng staff
            var staff = new TblStaff
            {
                StaId = Guid.NewGuid(),
                UacId = Guid.NewGuid(),
                StaName = staffDto.StaName,
                StaPhone = staffDto.StaPhone,
                StaRole = staffDto.StaRole,
                StaBaseSalary = staffDto.StaBaseSalary,
                CreatedBy = currentUserId,
                CreatedAt = currentTime
            };

            //Cập nhật thông tin ở bảng UserAccount
            var hasher = new PasswordHasher<TblUserAccount>();
            var userAccount = new TblUserAccount
            {
                UacId = staff.UacId,
                UacEmail = staffDto.StaEmail,
                UacRole = staffDto.StaRole,
                CreatedAt = currentTime,
                CreatedBy = currentUserId,
            };
            userAccount.UacPassword = hasher.HashPassword(userAccount, staffDto.StaPassword);

            await _userAccountRepository.InsertAsync(userAccount);
            await _staffRepository.InsertAsync(staff);
            return staffDto;
        }

        public async Task<UpdateStaffProfileDto> UpdateStaffProfileAsync(Guid id, UpdateStaffProfileDto staffProfileDto)
        {
            var staff = await _staffRepository.FindByIdAsync(id);
            if (staff == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E01);

            if (staffProfileDto.StaBaseSalary <= 0)
                throw new ErrorException(StatusCodeEnum.E04);

            if (string.IsNullOrWhiteSpace(staffProfileDto.StaPhone) || !Regex.IsMatch(staffProfileDto.StaPhone, @"^\d{10}$"))
                throw new ErrorException(StatusCodeEnum.E05);
            var currentUserId = GetCurrentUserId();
            var currentTime = ToGmt7(DateTime.UtcNow);

            staff.StaName = staffProfileDto.StaName;
            staff.StaPhone = staffProfileDto.StaPhone;
            staff.StaBaseSalary = staffProfileDto.StaBaseSalary;
            staff.StaRole = staffProfileDto.StaRole;
            staff.UpdatedBy = currentUserId;
            staff.UpdatedAt = currentTime;

            // Update bảng UserAccount (email + role, KHÔNG update mật khẩu ở đây)
            var userAccount = await _userAccountRepository.FindByIdAsync(staff.UacId);
            if (userAccount == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E03);

            userAccount.UacEmail = staffProfileDto.StaEmail;
            userAccount.UacRole = staffProfileDto.StaRole;
            userAccount.UpdatedBy = currentUserId;
            userAccount.UpdatedAt = currentTime;

            await _userAccountRepository.UpdateAsync(userAccount);
            await _staffRepository.UpdateAsync(staff);

            return new UpdateStaffProfileDto
            {
                StaName = staff.StaName,
                StaRole = staff.StaRole,
                StaPhone = staff.StaPhone,
                StaBaseSalary = (decimal)staff.StaBaseSalary,
                StaEmail = userAccount.UacEmail
            };
        }
        //Delete nhân viên
        public async Task DeleteStaffAsync(Guid id)
        {
            var staff = await _staffRepository.FindByIdAsync(id);
            if (staff == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E01);
            var userAccount = await _userAccountRepository.FindByIdAsync(staff.UacId);
            var currentUserId = GetCurrentUserId();
            var currentTime = ToGmt7(DateTime.UtcNow);

            userAccount.IsDeleted = true;
            staff.IsDeleted = true;
            staff.IsDeleted = true; // Soft delete
            staff.UpdatedBy = currentUserId;
            staff.UpdatedAt = currentTime;
            await _staffRepository.UpdateAsync(staff);
            await _userAccountRepository.UpdateAsync(userAccount);
        }

        public async Task<ApiResponseModel<OverviewReportDto>> GetOverviewReportAsync()
        {
            try
            {
                var data = await _staffRepository.GetOverviewReportAsync();

                if (data == null || (data.TotalStaff == 0 && data.ActiveStaffToday == 0))  // Ví dụ check error
                {
                    throw new ErrorException(StatusCodeEnum.PageIndexInvalid);  // Hoặc enum error phù hợp
                }

                return new ApiResponseModel<OverviewReportDto>(data);  // Success với data
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                throw new ErrorException(StatusCodeEnum.Error);  // Trả về error
            }
        }

        public async Task<ApiResponseModel<StaffDetailResponseDto>> GetStaffDetailReportAsync(StaffDetailRequestDto request)
        {
            // Validation (giữ nguyên hoặc mở rộng)
            if (request.Month > 12 || request.Month < 1 || request.Year < 1)
            {
                throw new ErrorException(StatusCodeEnum.BadRequest);  // Giả sử có enum BadRequest
            }

            try
            {
                var (details, total) = await _staffRepository.GetStaffDetailsAsync(request);
                var summary = await _staffRepository.GetSummaryAsync(request.Month, request.Year, request.StaffId, request.Role);

                if (details == null || details.Count == 0)
                {
                    throw new ErrorException(StatusCodeEnum.PageIndexInvalid);
                }

                var response = new StaffDetailResponseDto
                {
                    StaffDetails = details,
                    TotalCount = total,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    Summary = summary
                };

                return new ApiResponseModel<StaffDetailResponseDto>(response);  // Success
            }
            catch (Exception ex)
            {
                // Log error
                throw new ErrorException(StatusCodeEnum.Error);
            }
        }
    }
}
