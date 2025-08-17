namespace RestaurantManagement.Service.Implementation
{
    public class PromotionService : BaseService, IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PromotionService(AppSettings appSettings, IPromotionRepository promotionRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
            : base(appSettings, mapper, httpContextAccessor)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
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
            // Kiểm tra mã promotion đã tồn tại chưa (không phân biệt hoa thường)
            var isExist = await _promotionRepository.AnyAsync(x => x.ProCode.ToLower() == promotionDto.ProCode.ToLower() && !x.IsDeleted);
            if (isExist)
            {
                throw new ErrorException(StatusCodeEnum.D07, "Mã đã tồn tại");
            }
            var currentUserId = GetCurrentUserId();
            var currentTime = ToGmt7(DateTime.UtcNow);
            var promotion = new TblPromotion
            {
                ProId = Guid.NewGuid(),
                ProCode = promotionDto.ProCode,
                Description = promotionDto.Description,
                DiscountType = promotionDto.DiscountType,
                DiscountVal = promotionDto.DiscountVal,
                ConditionVal = promotionDto.ConditionVal,
                StartDate = promotionDto.StartDate,
                EndDate = promotionDto.EndDate,
                ProQuantity = promotionDto.ProQuantity,
                CreatedAt = currentTime,
                CreatedBy = currentUserId
            };
            await _promotionRepository.InsertAsync(promotion);
            return promotionDto;
        }

        public async Task<PromotionDto> UpdatePromotionAsync(Guid id, PromotionDto promotionDto)
        {
            var currentUserId = GetCurrentUserId();
            var currentTime = ToGmt7(DateTime.UtcNow);
            var promotion = await _promotionRepository.FindByIdAsync(id);
            if (promotion == null) throw new ErrorException(StatusCodeEnum.D04);

            promotion.ProCode = promotionDto.ProCode;
            promotion.Description = promotionDto.Description;
            promotion.DiscountType = promotionDto.DiscountType;
            promotion.DiscountVal = promotionDto.DiscountVal;
            promotion.ConditionVal = promotionDto.ConditionVal;
            promotion.StartDate = promotionDto.StartDate;
            promotion.EndDate = promotionDto.EndDate;
            promotion.UpdatedAt = currentTime;
            promotion.UpdatedBy = currentUserId;
            promotion.ProQuantity = promotionDto.ProQuantity;

            await _promotionRepository.UpdateAsync(promotion);
            return promotionDto;
        }

        public async Task<bool> DeletePromotionAsync(Guid id)
        {
            var promotion = await _promotionRepository.FindByIdAsync(id);
            if (promotion == null) return false;
            var currentUserId = GetCurrentUserId();
            var currentTime = ToGmt7(DateTime.UtcNow);

            await _promotionRepository.DeleteAsync(promotion);
            promotion.UpdatedAt = currentTime;
            promotion.UpdatedBy = currentUserId;
            await _promotionRepository.UpdateAsync(promotion);
            return true;
        }
    }
}