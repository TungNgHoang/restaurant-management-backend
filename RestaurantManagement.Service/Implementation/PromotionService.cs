using System.Net;
using System.Net.Mail;

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
            await SendPromotionEmailsAsync(promotion);
            return promotionDto;
        }
        private async Task SendPromotionEmailsAsync(TblPromotion promotion)
        {
            // Parse rank tối thiểu
            if (!Enum.TryParse<CustomerTierEnum>(promotion.DiscountType, out var minTier))
                return;

            // Lấy list rank >= minTier
            var eligibleTiers = Enum.GetValues(typeof(CustomerTierEnum))
                .Cast<CustomerTierEnum>()
                .Where(t => t >= minTier)
                .Select(t => t.ToString())
                .ToList();

            // Lọc khách hàng theo tier
            var customers = await _customerRepository.FilterAsync(c =>
                !c.IsDeleted &&
                !string.IsNullOrEmpty(c.CusEmail) &&
                eligibleTiers.Contains(c.CusTier));

            foreach (var customer in customers)
            {
                try
                {
                    using var smtp = new SmtpClient("smtp.gmail.com", 587)
                    {
                        EnableSsl = true,
                        Credentials = new NetworkCredential("lehaiphong4004@gmail.com", "excj xvmr fbmp oblb")
                    };

                    var mail = new MailMessage
                    {
                        From = new MailAddress("no-reply@yourdomain.com", "Restaurant"),
                        Subject = $"[Voucher mới] {promotion.ProCode}",
                        Body = $@"
                    <!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông báo Voucher từ Nhà hàng</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f4f4f4; padding: 20px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 20px; background-color: #4CAF50; border-radius: 8px 8px 0 0; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px;"">Chào mừng bạn nhận Voucher!</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;"">
                            <p style=""margin: 0 0 20px;"">Xin chào <strong>{customer.CusName ?? customer.CusEmail}</strong>,</p>
                            <p style=""margin: 0 0 20px;"">Chúng tôi rất vui thông báo bạn vừa nhận được một voucher đặc biệt từ nhà hàng:</p>
                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f9f9f9; padding: 20px; border-radius: 6px;"">
                                <tr>
                                    <td style=""font-size: 16px;"">
                                        <strong>Mã Voucher:</strong> <span style=""color: #4CAF50; font-weight: bold;"">{promotion.ProCode}</span><br/>
                                        <strong>Mô tả:</strong> {promotion.Description}<br/>
                                        <strong>Thời hạn:</strong> {promotion.StartDate:dd/MM/yyyy} – {promotion.EndDate:dd/MM/yyyy}<br/>
                                        <strong>Điều kiện:</strong> Đơn hàng từ {promotion.ConditionVal:N0}đ<br/>
                                        <strong>Số lượng:</strong> {promotion.ProQuantity}
                                    </td>
                                </tr>
                            </table>
                            <p style=""margin: 20px 0 0;"">Cảm ơn bạn đã luôn đồng hành cùng nhà hàng! Chúng tôi rất mong được phục vụ bạn.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px; background-color: #f1f1f1; border-radius: 0 0 8px 8px; text-align: center; color: #666666; font-size: 14px;"">
                            <p style=""margin: 0;"">Nhà hàng PizzaDaay<br/>
                            website: https://pizzadaay.ric.vn/ | Hotline: 0123 456 789</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                        IsBodyHtml = true
                    };
                    mail.To.Add(customer.CusEmail);

                    await smtp.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    // log tạm bằng Console hoặc logger nếu có
                    Console.WriteLine($"Không gửi được email cho {customer.CusEmail}: {ex.Message}");
                }
            }
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

        public async Task<List<PromotionDto>> GetAvailablePromotionAsync(Guid id)
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
            if (order == null)
            {
                throw new ErrorException(StatusCodeEnum.C07);
            }
            var currentTime = ToGmt7(DateTime.UtcNow);
            //Lấy danh sách khuyến mãi còn hiệu lực
            var promotionDb = await _promotionRepository.FilterAsync(p => 
                !p.IsDeleted && 
                p.StartDate <= currentTime && 
                p.EndDate >= currentTime && 
                p.ProQuantity > 0 &&
                p.ConditionVal <= order.TotalPrice);

            if (!Enum.TryParse<CustomerTierEnum>(customer.CusTier, out var cusTier))
            {
                throw new ErrorException(StatusCodeEnum.C09);
            }

            var promotions = promotionDb
                .Where(p =>
                    Enum.TryParse<CustomerTierEnum>(p.DiscountType, out var promoTier) &&
                    cusTier >= promoTier
                )
                .ToList();
            return _mapper.Map<List<PromotionDto>>(promotions);
        }
    }
}