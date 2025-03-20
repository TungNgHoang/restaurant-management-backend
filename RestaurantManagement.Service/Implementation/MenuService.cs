using AutoMapper;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Service.Implementation
{
    public class MenuService : BaseService, IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMapper _mapper;

        public MenuService(AppSettings appSettings, IMenuRepository menuRepository, IMapper mapper) : base(appSettings, mapper)
        {
            _menuRepository = menuRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MenuDto>> GetAllMenuAsync(MenuModels pagingModel)
        {
            // Validate PageIndex and PageSize
            ValidatePagingModel(pagingModel);

            var data = await _menuRepository.AsNoTrackingAsync();

            var menuDtos = _mapper.Map<List<MenuDto>>(data);
            var result = AdvancedFilter(menuDtos.AsEnumerable(), pagingModel, nameof(MenuDto.MnuName));

            return result;
        }

        private void ValidatePagingModel(MenuModels pagingModel)
        {
            if (pagingModel.PageIndex < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageIndexInvalid);
            if (pagingModel.PageSize < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageSizeInvalid);
        }
    }
}
