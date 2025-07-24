using RestaurantManagement.Service.Dtos.StaffDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class StaffService : BaseService, IStaffService
    {
        public readonly IStaffRepository _staffRepository;
        public readonly IMapper _mapper;
        public StaffService(AppSettings appSettings, IStaffRepository staffRepository, IMapper mapper) : base(appSettings, mapper)
        {
            _staffRepository = staffRepository;
            _mapper = mapper;
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
    }
}
