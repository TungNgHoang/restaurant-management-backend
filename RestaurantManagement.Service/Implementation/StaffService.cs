using RestaurantManagement.Service.Dtos.StaffDto;
using System.Text.RegularExpressions;

namespace RestaurantManagement.Service.Implementation
{
    public class StaffService : BaseService, IStaffService
    {
        public readonly IStaffRepository _staffRepository;
        public readonly IRepository<TblUserAccount> _userAccountRepository;
        public readonly IMapper _mapper;

        public StaffService(
            AppSettings appSettings, 
            IStaffRepository staffRepository, 
            IRepository<TblUserAccount> userAccountRepository, 
            IMapper mapper) : base(appSettings, mapper)
        {
            _staffRepository = staffRepository;
            _mapper = mapper;
            _userAccountRepository = userAccountRepository;
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
                StaBaseSalary = staffDto.StaBaseSalary
            };

            //Cập nhật thông tin ở bảng UserAccount
            var hasher = new PasswordHasher<TblUserAccount>();
            var userAccount = new TblUserAccount
            {
                UacId = staff.UacId,
                UacEmail = staffDto.StaEmail,
                UacRole = staffDto.StaRole
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

            staff.StaName = staffProfileDto.StaName;
            staff.StaPhone = staffProfileDto.StaPhone;
            staff.StaBaseSalary = staffProfileDto.StaBaseSalary;
            staff.StaRole = staffProfileDto.StaRole;

            // Update bảng UserAccount (email + role, KHÔNG update mật khẩu ở đây)
            var userAccount = await _userAccountRepository.FindByIdAsync(staff.UacId);
            if (userAccount == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E03);

            userAccount.UacEmail = staffProfileDto.StaEmail;
            userAccount.UacRole = staffProfileDto.StaRole;

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

            userAccount.IsDeleted = true;
            staff.IsDeleted = true;
            await _staffRepository.UpdateAsync(staff);
            await _userAccountRepository.UpdateAsync(userAccount);
        }
    }
}
