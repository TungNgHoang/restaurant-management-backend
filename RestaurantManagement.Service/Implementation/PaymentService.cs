namespace RestaurantManagement.Service.Implementation
{
    public class PaymentService : BaseService, IPaymentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblTableInfo> _tablesRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<TblPayment> _paymentRepository;
        private readonly IRepository<TblPromotion> _promotionRepository;
        private readonly IRepository<TblCustomer> _customerRepository;
        protected readonly RestaurantDBContext _dbContext;
        public PaymentService(
            AppSettings appSettings,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IRepository<TblReservation> reservationsRepository,
            IRepository<TblTableInfo> tablesRepository,
            IOrderRepository orderRepository,
            IRepository<TblPayment> paymentRepositor,
            IRepository<TblPromotion> promotionRepository,
            IRepository<TblCustomer> customerRepository,
            RestaurantDBContext dbContext
            ) : base(appSettings, mapper, httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _reservationsRepository = reservationsRepository;
            _tablesRepository = tablesRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepositor;
            _promotionRepository = promotionRepository;
            _customerRepository = customerRepository;
        }

        public async Task CheckoutAndPayAsync(Guid resId, Guid ordId, string proCode, string payMethod)
        {
            // 1. Lấy thông tin đơn hàng
            var order = await _orderRepository.GetOrderByIdAsync(ordId);
            if (order == null)
                throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

            // 1. Kiểm tra reservation
            var reservation = await _reservationsRepository.FindByIdAsync(resId);
            if (reservation == null || reservation.ResStatus != ReservationStatus.Serving.ToString())
                throw new ErrorException(StatusCodeEnum.A03);

            // 2. Kiểm tra bàn
            var table = await _tablesRepository.FindByIdAsync(reservation.TbiId);
            if (table == null || table.TbiStatus != TableStatus.Occupied.ToString())
                throw new ErrorException(StatusCodeEnum.A04);
            // 3. Lấy thông tin khách hàng từ reservation
            if (!reservation.CusId.HasValue)
            {
                throw new ErrorException(StatusCodeEnum.C09);
            }
            var customer = await _customerRepository.FindByIdAsync(reservation.CusId.Value);
            if (customer == null)
                throw new ErrorException(StatusCodeEnum.C09);

            // Giảm giá theo mã khuyến mãi

            decimal originalPrice = order.TotalPrice;
            decimal priceAfterVoucher = originalPrice;
            decimal voucherDiscount = 0;
            decimal rankDiscount = 0;
            if (!string.IsNullOrEmpty(proCode))
            {
                // tìm promotion theo mã theo hàm FilterAsync
                var promotionList = await _promotionRepository.FilterAsync(p => p.ProCode == proCode && !p.IsDeleted && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);
                var promotion = promotionList.FirstOrDefault();
                if (promotion == null)
                    throw new ErrorException(StatusCodeEnum.D04);

                // Kiểm tra hạng
                if (Enum.TryParse<CustomerTierEnum>(customer.CusTier, out var customerTier) &&
                    Enum.TryParse<CustomerTierEnum>(promotion.DiscountType, out var requiredTier) &&
                    customerTier >= requiredTier)
                {
                    // Kiểm tra điều kiện
                    if (promotion.ConditionVal.HasValue && order.TotalPrice < promotion.ConditionVal.Value)
                    {
                        throw new ErrorException(StatusCodeEnum.D08);
                    }

                    //Kiểm tra số lượng khuyến mãi
                    if (promotion.ProQuantity <= 0)
                    {
                        throw new ErrorException(StatusCodeEnum.D09);
                    }

                    // Áp dụng giảm từ voucher
                    if (promotion.DiscountVal <= 1)
                    {
                        voucherDiscount = originalPrice * promotion.DiscountVal;
                    }
                    else
                    {
                        voucherDiscount = promotion.DiscountVal;
                    }

                    // Không vượt quá giá trị đơn hàng
                    voucherDiscount = Math.Min(voucherDiscount, originalPrice);
                    priceAfterVoucher -= voucherDiscount;
                }
                else
                {
                    throw new ErrorException(StatusCodeEnum.D07);
                }

            }

            // 2. Giảm thêm theo hạng khách hàng
            if (Enum.TryParse<CustomerTierEnum>(customer.CusTier, out var tier))
            {
                var tierDiscountMap = new Dictionary<CustomerTierEnum, decimal>
                {
                    { CustomerTierEnum.Standard, 0.02m },
                    { CustomerTierEnum.Silver,   0.05m },
                    { CustomerTierEnum.Gold,     0.07m },
                    { CustomerTierEnum.Diamond,  0.10m }
                };

                if (tierDiscountMap.TryGetValue(tier, out var rankPercent))
                {
                    rankDiscount = priceAfterVoucher * rankPercent;
                    priceAfterVoucher -= rankDiscount;
                }
            }
            var vat = 0.08m; // Giả sử thuế VAT là 8%
            var priceAfterVat = priceAfterVoucher * (1 + vat);
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUserId = GetCurrentUserId();
                    var currentTime = ToGmt7(DateTime.UtcNow);
                    // 4. Tạo bản ghi thanh toán
                    var payment = new TblPayment
                    {
                        PayId = Guid.NewGuid(),
                        OrdId = order.OrdId,
                        CusId = order.CusId,
                        Amount = priceAfterVat,
                        PayMethod = payMethod,
                        PayStatus = "Completed",
                        IsDeleted = false,
                        CreatedAt = currentTime,
                        CreatedBy = currentUserId
                    };
                    await _paymentRepository.InsertAsync(payment);

                    // 5. Cập nhật trạng thái reservation
                    reservation.ResStatus = ReservationStatus.Finished.ToString() /*"Finished",*/;
                    reservation.UpdatedAt = currentTime;
                    reservation.UpdatedBy = currentUserId;
                    await _reservationsRepository.UpdateAsync(reservation);

                    // 6. Cập nhật trạng thái bàn
                    table.TbiStatus = TableStatus.Empty.ToString(); //"Empty"
                    table.UpdatedAt = currentTime;
                    table.UpdatedBy = currentUserId; // Thay bằng ID nhân viên nếu cần
                    await _tablesRepository.UpdateAsync(table);

                    // 7. Cập nhật trạng thái voucher (giảm số lượng đi 1)
                    if (!string.IsNullOrEmpty(proCode))
                    {
                        var promotion = await _promotionRepository.FindAsync(p => p.ProCode == proCode && !p.IsDeleted);
                        if (promotion != null)
                        {
                            promotion.ProQuantity -= 1;
                            if (promotion.ProQuantity <= 0)
                            {
                                promotion.IsDeleted = true; // Xóa nếu hết số lượng
                            }
                            await _promotionRepository.UpdateAsync(promotion);
                        }
                    }

                    //Cập nhật điểm và lên hạng cho khách
                    customer.CusPoints += (int)priceAfterVoucher;
                    if (customer.CusPoints >= 10_000_000)
                        customer.CusTier = CustomerTierEnum.Diamond.ToString();
                    else if (customer.CusPoints >= 5_000_000)
                        customer.CusTier = CustomerTierEnum.Gold.ToString();
                    else if (customer.CusPoints >= 2_000_000)
                        customer.CusTier = CustomerTierEnum.Silver.ToString();
                    else if (customer.CusPoints >= 1_000_000)
                        customer.CusTier = CustomerTierEnum.Standard.ToString();
                    else
                        customer.CusTier = CustomerTierEnum.Unranked.ToString();

                    await _customerRepository.UpdateAsync(customer);

                    // Nếu cả hai thành công, commit transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Lỗi khi check-out: {ex.Message}");
                }
            }
        }
    }
}
