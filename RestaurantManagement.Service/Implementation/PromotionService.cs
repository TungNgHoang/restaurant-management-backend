using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.PromotionDto;
using RestaurantManagement.Service.Interfaces;

namespace RestaurantManagement.Service.Implementation
{
    public class PromotionService : BaseService, IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public PromotionService(AppSettings appSettings, IPromotionRepository promotionRepository, IMapper mapper)
            : base(appSettings, mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync(PromotionModels pagingModel)
        {
            ValidatePagingModel(pagingModel);

            var data = await _promotionRepository.AsNoTrackingAsync();
            var promotionDtos = _mapper.Map<List<PromotionDto>>(data);
            var result = AdvancedFilter(promotionDtos.AsEnumerable(), pagingModel, nameof(PromotionDto.Description));

            return result;
        }

        private void ValidatePagingModel(PromotionModels pagingModel)
        {
            if (pagingModel.PageIndex < 1)
                throw new ErrorException(StatusCodeEnum.PageIndexInvalid);
            if (pagingModel.PageSize < 1)
                throw new ErrorException(StatusCodeEnum.PageSizeInvalid);
        }

        public async Task<PromotionDto> GetPromotionByIdAsync(Guid id)
        {
            var promotion = await _promotionRepository.FindByIdAsync(id);
            return promotion != null ? _mapper.Map<PromotionDto>(promotion) : null;
        }

        public async Task<PromotionDto> AddPromotionAsync(PromotionDto promotionDto)
        {
            var promotion = new TblPromotion
            {
                ProId = Guid.NewGuid(),
                ProCode = promotionDto.ProCode,
                Description = promotionDto.Description,
                DiscountType = promotionDto.DiscountType,
                DiscountVal = promotionDto.DiscountVal,
                ConditionVal = promotionDto.ConditionVal,
                StartDate = promotionDto.StartDate,
                EndDate = promotionDto.EndDate
            };
            await _promotionRepository.InsertAsync(promotion);
            return promotionDto;
        }

        public async Task<PromotionDto> UpdatePromotionAsync(Guid id, PromotionDto promotionDto)
        {
            var promotion = await _promotionRepository.FindByIdAsync(id);
            if (promotion == null) throw new ErrorException(StatusCodeEnum.D04);

            promotion.ProCode = promotionDto.ProCode;
            promotion.Description = promotionDto.Description;
            promotion.DiscountType = promotionDto.DiscountType;
            promotion.DiscountVal = promotionDto.DiscountVal;
            promotion.ConditionVal = promotionDto.ConditionVal;
            promotion.StartDate = promotionDto.StartDate;
            promotion.EndDate = promotionDto.EndDate;
            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.UpdatedBy = Guid.NewGuid();

            await _promotionRepository.UpdateAsync(promotion);
            return promotionDto;
        }

        public async Task<bool> DeletePromotionAsync(Guid id)
        {
            var promotion = await _promotionRepository.FindByIdAsync(id);
            if (promotion == null) return false;

            await _promotionRepository.DeleteAsync(promotion);
            return true;
        }
    }
}