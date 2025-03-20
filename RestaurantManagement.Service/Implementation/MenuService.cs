using AutoMapper;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
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

        //Get món bằng ID
        public async Task<MenuDto> GetMenuByIdAsync(Guid id)
        {
            var menu = await _menuRepository.FindByIdAsync(id);
            return menu != null ? _mapper.Map<MenuDto>(menu) : null;
        }
        //Thêm món vào Menu
        public async Task<MenuDto> AddMenuAsync(MenuDto menuDto)
        {
            var menu = new TblMenu
            {
                MnuId = Guid.NewGuid(),
                MnuName = menuDto.MnuName,
                MnuPrice = menuDto.MnuPrice,
                MnuStatus = menuDto.MnuStatus,
                MnuImage = menuDto.MnuImage,
                MnuDescription = menuDto.MnuDescription
            };

            await _menuRepository.InsertAsync(menu);
            return menuDto;
        }

        //Update thông tin món
        public async Task<MenuDto> UpdateMenuAsync(Guid id, MenuDto menuDto)
        {
            var menu = await _menuRepository.FindByIdAsync(id);
            if (menu == null) throw new ErrorException(StatusCodeEnum.D01);

            menu.MnuName = menuDto.MnuName;
            menu.MnuPrice = menuDto.MnuPrice;
            menu.MnuStatus = menuDto.MnuStatus;
            menu.MnuImage = menuDto.MnuImage;
            menu.MnuDescription = menuDto.MnuDescription;

            await _menuRepository.UpdateAsync(menu);
            return menuDto;
        }
        //Xoá món 
        public async Task<bool> DeleteMenuAsync(Guid id)
        {
            var menu = await _menuRepository.FindByIdAsync(id);
            if (menu == null) return false;

            await _menuRepository.DeleteAsync(menu);
            return true;
        }
    }
}
