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

        public async Task<IEnumerable<StaffDto>> GetAllStaffAsync(StaffModels pagingModel)
        {
            // Validate PageIndex and PageSize
            ValidatePagingModel(pagingModel);

            var data = await _staffRepository.AsNoTrackingAsync();

            var staffDtos = _mapper.Map<List<StaffDto>>(data);
            var result = AdvancedFilter(staffDtos.AsEnumerable(), pagingModel, nameof(StaffDto.StaName));

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
        public async Task<StaffDto> GetStaffByIdAsync(Guid id)
        {
            var staff = await _staffRepository.FindByIdAsync(id);
            if (staff == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E01);
            return _mapper.Map<StaffDto>(staff);
        }
        //Thêm nhân viên
        public async Task<StaffDto> AddStaffAsync(StaffDto staffDto)
        {
            //Từ UacId lấy ra Role trong bảng TblUserAccount
            var userAccount = await _userAccountRepository.FindByIdAsync(staffDto.UacID);
            if (userAccount == null)
                throw new ErrorException(StatusCodeEnum.E03);
            if (staffDto.StaBaseSalary <= 0)
                throw new ErrorException(StatusCodeEnum.E04);
            if (string.IsNullOrWhiteSpace(staffDto.StaPhone) || !Regex.IsMatch(staffDto.StaPhone, @"^\d{10}$"))
                throw new ErrorException(StatusCodeEnum.E05);
            var staff = new TblStaff
            {
                StaId = Guid.NewGuid(),
                UacId = staffDto.UacID,
                StaName = staffDto.StaName,
                StaRole = userAccount.UacRole,
                StaPhone = staffDto.StaPhone,
                StaBaseSalary = staffDto.StaBaseSalary
            };

            await _staffRepository.InsertAsync(staff);
            return staffDto;
        }
        //Update thông tin nhân viên
        public async Task<StaffDto> UpdateStaffAsync(StaffDto staffDto)
        {
            //Từ UacId lấy ra Role trong bảng TblUserAccount
            var userAccount = await _userAccountRepository.FindByIdAsync(staffDto.UacID);
            if (userAccount == null)
                throw new ErrorException(StatusCodeEnum.E03);
            var staff = await _staffRepository.FindByIdAsync(staffDto.StaID);
            if (staff == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E01);
            if (staffDto.StaBaseSalary <= 0)
                throw new ErrorException(StatusCodeEnum.E04);
            if (string.IsNullOrWhiteSpace(staffDto.StaPhone) || !Regex.IsMatch(staffDto.StaPhone, @"^\d{10}$"))
                throw new ErrorException(StatusCodeEnum.E05);

            staff.UacId = staffDto.UacID;
            staff.StaName = staffDto.StaName;
            staff.StaRole = userAccount.UacRole;
            staff.StaPhone = staffDto.StaPhone;
            staff.StaBaseSalary = staffDto.StaBaseSalary;

            await _staffRepository.UpdateAsync(staff);
            return _mapper.Map<StaffDto>(staff);
        }
        //Delete nhân viên
        public async Task DeleteStaffAsync(Guid id)
        {
            var staff = await _staffRepository.FindByIdAsync(id);
            if (staff == null)
                throw new ErrorException(Core.Enums.StatusCodeEnum.E01);

            staff.IsDeleted = true; // Soft delete
            await _staffRepository.UpdateAsync(staff);
        }
    }
}
