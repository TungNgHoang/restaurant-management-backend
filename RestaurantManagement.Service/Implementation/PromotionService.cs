namespace RestaurantManagement.Service.Implementation
{
    public class PromotionService : BaseService, IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<TblReservation> _reservationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<TblCustomer> _customerRepository;

        public PromotionService(AppSettings appSettings, 
            IRepository<TblReservation> reservationRepository, 
            IOrderRepository orderRepository, 
            IRepository<TblCustomer> customerRepository,
            IPromotionRepository promotionRepository, 
            IMapper mapper, IHttpContextAccessor httpContextAccessor)
            : base(appSettings, mapper, httpContextAccessor)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _reservationRepository = reservationRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
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

        public async Task<PromotionDto> GetAvailablePromotionAsync(Guid id)
        {
            //Lấy thông tin đơn hàng
            var reservation = await _reservationRepository.FindByIdAsync(id);
            if (reservation == null)
            {
                throw new ErrorException(StatusCodeEnum.ReservatioNotFound);
            }

            //Lấy thông tin khách hàng từ reservation
            if (!reservation.CusId.HasValue)
            {
                throw new ErrorException(StatusCodeEnum.C09);
            }
            var customer = await _customerRepository.FindByIdAsync(reservation.CusId.Value);
            if (customer == null)
            {
                throw new ErrorException(StatusCodeEnum.C09);
            }
            //Lấy thông tin đơn hàng, tìm trong bảng tblOrderInfo, bản ghi nào có ResId trùng với reservation.Id
            var order = await _orderRepository.GetOrderByResIdAsync(reservation.ResId);

        }
    }
}