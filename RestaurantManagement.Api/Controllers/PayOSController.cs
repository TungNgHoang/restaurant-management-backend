using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Net.payOS.Types;
using RestaurantManagement.Service.Dtos.PaymentDto;

namespace RestaurantManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayOSController : BaseApiController
    {
        private readonly IPayOSService _payOSService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PayOSController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblTableInfo> _tablesRepository;
        private readonly IRepository<TblCustomer> _customerRepository;
        private readonly IRepository<TblPromotion> _promotionRepository;
        private readonly RestaurantDBContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly IRepository<TblPayment> _paymentRepository;

        public PayOSController(
            IServiceProvider serviceProvider,
            IPayOSService payOSService,
            IPaymentService paymentService,
            ILogger<PayOSController> logger,
            IOrderRepository orderRepository,
            IRepository<TblReservation> reservationsRepository,
            IRepository<TblTableInfo> tablesRepository,
            IRepository<TblCustomer> customerRepository,
            IRepository<TblPromotion> promotionRepository,
            RestaurantDBContext dbContext,
            INotificationService notificationService,
            IRepository<TblPayment> paymentRepository) : base(serviceProvider)
        {
            _payOSService = payOSService;
            _paymentService = paymentService;
            _logger = logger;
            _orderRepository = orderRepository;
            _reservationsRepository = reservationsRepository;
            _tablesRepository = tablesRepository;
            _customerRepository = customerRepository;
            _promotionRepository = promotionRepository;
            _dbContext = dbContext;
            _notificationService = notificationService;
            _paymentRepository = paymentRepository;
        }

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] PaymentLinkRequest request, string? proCode)
        {
            try
            {
                // 1. Lấy thông tin đơn hàng
                var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
                if (order == null)
                    throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

                // 1. Kiểm tra reservation
                var reservation = await _reservationsRepository.FindByIdAsync(request.ReservationId);
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
                        Console.WriteLine(customerTier);
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
                // Tạo orderCode duy nhất
                long orderCode = PayOSService.GenerateUniqueOrderCode(request.OrderId);

                // Tạo danh sách items với mỗi PaymentItem trong request
                var items = request.Items.Select(i => new ItemData(
                    name: i.Name,
                    quantity: 1,
                    price: i.Price
                )).ToList();

                // Tạo PaymentData
                var paymentData = new PaymentData(
                    orderCode: orderCode,
                    amount: (int)priceAfterVat,
                    description: orderCode.ToString(),
                    items: items,
                    cancelUrl: "https://pizzadaay.ric.vn/admin-reservation",
                    returnUrl: "https://pizzadaay.ric.vn/payment/success"
                );

                // Gọi service tạo payment link
                var result = await _payOSService.CreatePaymentLinkAsync(paymentData);

                return Ok(new PaymentLinkResponse
                {
                    Message = "Tạo payment link thành công",
                    Data = new PaymentLinkData
                    {
                        OrderCode = orderCode,
                        CheckoutUrl = result.checkoutUrl,
                        QrCode = result.qrCode,
                        Amount = result.amount,
                        Description = result.description,
                        Status = result.status,
                        PaymentLinkId = result.paymentLinkId,
                        AccountNumber = result.accountNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demo tạo payment link thất bại");
                return BadRequest(new PaymentLinkResponse
                {
                    Error = ex.Message,
                    Message = "Tạo payment link thất bại",
                    Data = null
                });
            }
        }


        [HttpGet("get-payment-info/{orderCode}")]
        public async Task<IActionResult> GetPaymentInfo(long orderCode)
        {
            try
            {
                var paymentInfo = await _payOSService.GetPaymentLinkInformationAsync(orderCode);

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin payment thành công",
                    data = new PayOSOrderStatusDto
                    {
                        //map các trường từ paymentInfo sang PayOSOrderStatusDto
                        OrderCode = paymentInfo.orderCode,
                        Status = paymentInfo.status,
                        Amount = paymentInfo.amount,
                        CreatedAt = DateTime.Parse(paymentInfo.createdAt),
                        PaidAt = paymentInfo.transactions != null && paymentInfo.transactions.Count > 0 ? (DateTime?)DateTime.Parse(paymentInfo.transactions[0].transactionDateTime) : null,
                        TransactionId = paymentInfo.transactions.ToString(),
                        PaymentMethod = "PayOS"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demo lấy thông tin payment thất bại cho OrderCode: {OrderCode}", orderCode);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("cancel-payment/{orderCode}")]
        public async Task<IActionResult> CancelPayment(long orderCode, [FromBody] CancelPaymentLinkRequest request)
        {
            try
            {
                var cancelledPayment = await _payOSService.CancelPaymentLinkAsync(orderCode, request.Reason);

                return Ok(new
                {
                    success = true,
                    message = "Hủy payment thành công",
                    data = new PayOSOrderStatusDto
                    {
                        OrderCode = cancelledPayment.orderCode,
                        Status = cancelledPayment.status,
                        Amount = cancelledPayment.amount,
                        Description = cancelledPayment.cancellationReason
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demo hủy payment thất bại cho OrderCode: {OrderCode}", orderCode);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook([FromBody] string webhookUrl)
        {
            try
            {
                var result = await _payOSService.ConfirmWebhookAsync(webhookUrl);
                
                return Ok(new
                {
                    success = true,
                    message = "Xác thực webhook thành công",
                    data = new
                    {
                        webhookUrl = webhookUrl,
                        result = result
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demo xác thực webhook thất bại cho URL: {WebhookUrl}", webhookUrl);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("webhook")]
        public IActionResult WebhookHandler([FromBody] WebhookType webhookBody)
        {
            try
            {
                // Xác minh webhook data
                var verifiedData = _payOSService.VerifyPaymentWebhookData(webhookBody);

                _logger.LogInformation("Nhận webhook thành công - OrderCode: {OrderCode}, Amount: {Amount}, Status: {Code}",
                    verifiedData.orderCode, verifiedData.amount, verifiedData.code);

                // Xử lý logic business ở đây
                // Ví dụ: Cập nhật database, gửi email, thông báo, etc.

                return Ok(new
                {
                    success = true,
                    message = "Xử lý webhook thành công",
                    data = new
                    {
                        orderCode = verifiedData.orderCode,
                        amount = verifiedData.amount,
                        description = verifiedData.description,
                        accountNumber = verifiedData.accountNumber,
                        reference = verifiedData.reference,
                        transactionDateTime = verifiedData.transactionDateTime,
                        code = verifiedData.code,
                        desc = verifiedData.desc
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demo xử lý webhook thất bại");
                return Ok(new { success = false, message = "Webhook processing failed" });
            }
        }
    }
}
