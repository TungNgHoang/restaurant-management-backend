using Elastic.Clients.Elasticsearch.Inference;
using Net.payOS.Types;
using RestaurantManagement.Service.Dtos.PaymentDto;
using RestaurantManagement.Service.Interfaces;
using System.Security.Cryptography;

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
        private readonly INotificationService _notificationService;
        protected readonly RestaurantDBContext _dbContext;
        private readonly ILogger<PaymentService> _logger;
        private readonly IPayOSService _payOSService;
        private readonly IInvoiceService _invoiceService;
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
            INotificationService notificationService,
            RestaurantDBContext dbContext,
            ILogger<PaymentService> logger,
            IPayOSService payOSService,
            IInvoiceService invoiceService
            ) : base(appSettings, mapper, httpContextAccessor, dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _reservationsRepository = reservationsRepository;
            _tablesRepository = tablesRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepositor;
            _promotionRepository = promotionRepository;
            _customerRepository = customerRepository;
            _notificationService = notificationService;
            _logger = logger;
            _payOSService = payOSService;
            _invoiceService = invoiceService;
        }

        public async Task<byte[]> CheckoutAndPayAsync(Guid resId, Guid ordId, string proCode, string payMethod)
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
                    table.UpdatedBy = currentUserId; 
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
                    var invoice = await _invoiceService.GenerateInvoicePdf(ordId);
                    // Nếu cả hai thành công, commit transaction
                    await transaction.CommitAsync();

                    // Send payment success notification
                    try
                    {
                        await _notificationService.SendPaymentSuccessNotificationAsync(
                            resId, priceAfterVat, customer.CusName ?? reservation.TempCustomerName ?? "Khách hàng");

                        _logger.LogInformation("Đã gửi thông báo thanh toán thành công cho ResId: {ResId}", resId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Lỗi khi gửi thông báo thanh toán thành công cho ResId: {ResId}", resId);
                    }
                    return invoice;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Lỗi khi check-out: {ex.Message}");
                }
            }
        }
        //public async Task<PayOSPaymentResponseDto> CreatePayOSPaymentAsync(Guid resId, Guid ordId, string? proCode)
        //{
        //    // 1. Lấy thông tin đơn hàng
        //    var order = await _orderRepository.GetOrderByIdAsync(ordId);
        //    if (order == null)
        //        throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

        //    // 2. Kiểm tra reservation
        //    var reservation = await _reservationsRepository.FindByIdAsync(resId);
        //    if (reservation == null || reservation.ResStatus != ReservationStatus.Serving.ToString())
        //        throw new ErrorException(StatusCodeEnum.A03);

        //    // 3. Lấy thông tin khách hàng
        //    if (!reservation.CusId.HasValue)
        //        throw new ErrorException(StatusCodeEnum.C09);

        //    var customer = await _customerRepository.FindByIdAsync(reservation.CusId.Value);
        //    if (customer == null)
        //        throw new ErrorException(StatusCodeEnum.C09);

        //    // 4. Tính toán giá sau giảm giá (sử dụng logic hiện tại)
        //    decimal originalPrice = order.TotalPrice;
        //    decimal priceAfterVoucher = originalPrice;
        //    decimal voucherDiscount = 0;

        //    // Logic giảm giá giống như trong CheckoutAndPayAsync
        //    if (!string.IsNullOrEmpty(proCode))
        //    {
        //        var promotionList = await _promotionRepository.FilterAsync(p => p.ProCode == proCode && !p.IsDeleted && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);
        //        var promotion = promotionList.FirstOrDefault();
        //        if (promotion != null)
        //        {
        //            if (Enum.TryParse<CustomerTierEnum>(customer.CusTier, out var customerTier) &&
        //                Enum.TryParse<CustomerTierEnum>(promotion.DiscountType, out var requiredTier) &&
        //                customerTier >= requiredTier)
        //            {
        //                if (!promotion.ConditionVal.HasValue || order.TotalPrice >= promotion.ConditionVal.Value)
        //                {
        //                    if (promotion.ProQuantity > 0)
        //                    {
        //                        if (promotion.DiscountVal <= 1)
        //                        {
        //                            voucherDiscount = originalPrice * promotion.DiscountVal;
        //                        }
        //                        else
        //                        {
        //                            voucherDiscount = promotion.DiscountVal;
        //                        }
        //                        voucherDiscount = Math.Min(voucherDiscount, originalPrice);
        //                        priceAfterVoucher -= voucherDiscount;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // 5. Giảm giá theo hạng khách hàng
        //    if (Enum.TryParse<CustomerTierEnum>(customer.CusTier, out var tier))
        //    {
        //        var tierDiscountMap = new Dictionary<CustomerTierEnum, decimal>
        //        {
        //            { CustomerTierEnum.Standard, 0.02m },
        //            { CustomerTierEnum.Silver, 0.05m },
        //            { CustomerTierEnum.Gold, 0.07m },
        //            { CustomerTierEnum.Diamond, 0.10m }
        //        };

        //        if (tierDiscountMap.TryGetValue(tier, out var rankPercent))
        //        {
        //            var rankDiscount = priceAfterVoucher * rankPercent;
        //            priceAfterVoucher -= rankDiscount;
        //        }
        //    }

        //    // 6. Thêm VAT
        //    var vat = 0.08m;
        //    var finalAmount = priceAfterVoucher * (1 + vat);

        //    // 9. Lưu thông tin payment với trạng thái Pending
        //    var currentUserId = GetCurrentUserId();
        //    var currentTime = ToGmt7(DateTime.UtcNow);

        //    // Tạo payment record với status "Pending" 
        //    var payment = new TblPayment
        //    {
        //        PayId = Guid.NewGuid(),
        //        OrdId = ordId,
        //        CusId = order.CusId,
        //        Amount = finalAmount,
        //        PayMethod = "PayOS",
        //        PayStatus = "Pending",
        //        IsDeleted = false,
        //        CreatedAt = ToGmt7(DateTime.UtcNow),
        //        CreatedBy = GetCurrentUserId()
        //    };

        //    await _paymentRepository.InsertAsync(payment);

        //    // Tạo PayOS payment request và trả về response
        //    var payosRequest = new PayOSPaymentRequestDto {
        //        ResId = resId,
        //        OrdId = ordId,
        //        Amount = finalAmount,
        //        Description = ordId.ToString(),
        //        ProCode = proCode,
        //        CustomerName = customer.CusName ?? reservation.TempCustomerName ?? "Khách hàng",
        //        CustomerEmail = customer.CusEmail
        //    };
        //    var payosResult = await _payOSService.CreatePaymentLinkAsync(payosRequest);

        //    return new PayOSPaymentResponseDto {
        //        CheckoutUrl = payosResult.checkoutUrl,
        //        QrCode = payosResult.qrCode,
        //        OrderCode = payosResult.orderCode,
        //        PaymentLinkId = payosResult.paymentLinkId,
        //        Status = payosResult.status,
        //        Amount = payosResult.amount,
        //        AccountNumber = payosResult.accountNumber
        //    };
        //}

        //public async Task HandlePayOSWebhookAsync(WebhookType webhookData)
        //{
        //    try
        //    {
        //        // 1. Verify webhook data
        //        var verifiedData = await _payOSService.VerifyWebhookDataAsync(webhookData);

        //        if (!webhookData.success)
        //        {
        //            _logger.LogWarning("PayOS webhook received with failure status for order {OrderCode}", verifiedData.orderCode);
        //            return;
        //        }

        //        // 2. Tìm payment record
                
        //        var payment = await _paymentRepository.FindAsync(p => p.OrdId.ToString() == verifiedData.description && p.PayMethod == "PayOS");

        //        if (payment == null)
        //        {
        //            _logger.LogWarning("Payment not found for PayOS order {OrderCode}", verifiedData.orderCode);
        //            return;
        //        }

        //        // 3. Cập nhật trạng thái payment
        //        payment.PayStatus = "Completed";
        //        payment.UpdatedAt = ToGmt7(DateTime.UtcNow);
        //        payment.UpdatedBy = GetCurrentUserId();

        //        await _paymentRepository.UpdateAsync(payment);

        //        // 4. Cập nhật trạng thái reservation và table (logic từ CheckoutAndPayAsync)
        //        var order = await _orderRepository.GetOrderByIdAsync(payment.OrdId);
        //        if (order != null)
        //        {
        //            var reservation = await _reservationsRepository.FindAsync(r => r.ResId == order.ResId);
        //            if (reservation != null)
        //            {
        //                var currentUserId = GetCurrentUserId();
        //                var currentTime = ToGmt7(DateTime.UtcNow);

        //                // Cập nhật reservation
        //                reservation.ResStatus = ReservationStatus.Finished.ToString();
        //                reservation.UpdatedAt = currentTime;
        //                reservation.UpdatedBy = currentUserId;
        //                await _reservationsRepository.UpdateAsync(reservation);

        //                // Cập nhật table
        //                var table = await _tablesRepository.FindByIdAsync(reservation.TbiId);
        //                if (table != null)
        //                {
        //                    table.TbiStatus = TableStatus.Empty.ToString();
        //                    table.UpdatedAt = currentTime;
        //                    table.UpdatedBy = currentUserId;
        //                    await _tablesRepository.UpdateAsync(table);
        //                }

        //                // Cập nhật customer points và tier
        //                if (reservation.CusId.HasValue)
        //                {
        //                    var customer = await _customerRepository.FindByIdAsync(reservation.CusId.Value);
        //                    if (customer != null)
        //                    {
        //                        customer.CusPoints += (int)payment.Amount;

        //                        // Update tier logic
        //                        if (customer.CusPoints >= 10_000_000)
        //                            customer.CusTier = CustomerTierEnum.Diamond.ToString();
        //                        else if (customer.CusPoints >= 5_000_000)
        //                            customer.CusTier = CustomerTierEnum.Gold.ToString();
        //                        else if (customer.CusPoints >= 2_000_000)
        //                            customer.CusTier = CustomerTierEnum.Silver.ToString();
        //                        else if (customer.CusPoints >= 1_000_000)
        //                            customer.CusTier = CustomerTierEnum.Standard.ToString();
        //                        else
        //                            customer.CusTier = CustomerTierEnum.Unranked.ToString();

        //                        await _customerRepository.UpdateAsync(customer);
        //                    }
        //                }

        //                // Send notification
        //                try
        //                {
        //                    var customer = await _customerRepository.FindByIdAsync(reservation.CusId.Value);
        //                    await _notificationService.SendPaymentSuccessNotificationAsync(
        //                        reservation.ResId, payment.Amount, customer?.CusName ?? reservation.TempCustomerName ?? "Khách hàng");
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.LogWarning(ex, "Failed to send payment success notification");
        //                }
        //            }
        //        }

        //        _logger.LogInformation("PayOS payment processed successfully for order {OrderCode}", verifiedData.orderCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing PayOS webhook for order {OrderCode}", webhookData.data?.orderCode);
        //        throw;
        //    }
        //}
    }
}
