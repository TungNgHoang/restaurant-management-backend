using Net.payOS;
using Net.payOS.Types;
using System;

namespace RestaurantManagement.Service.Implementation
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly ILogger<PayOSService> _logger;

        
        public PayOSService(PayOS payOS, ILogger<PayOSService> logger)
        {
            _payOS = payOS ?? throw new ArgumentNullException(nameof(payOS));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        public async Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData)
        {
            try
            {
                _logger.LogInformation("Bắt đầu tạo PayOS payment link cho OrderCode: {OrderCode}, Amount: {Amount}",
                    paymentData.orderCode, paymentData.amount);

                // Validate input data
                if (paymentData == null)
                {
                    throw new ArgumentNullException(nameof(paymentData), "PaymentData không được null");
                }

                if (paymentData.orderCode <= 0)
                {
                    throw new ArgumentException("OrderCode phải là số dương", nameof(paymentData.orderCode));
                }

                if (paymentData.amount <= 0)
                {
                    throw new ArgumentException("Amount phải lớn hơn 0", nameof(paymentData.amount));
                }

                if (string.IsNullOrWhiteSpace(paymentData.description))
                {
                    throw new ArgumentException("Description không được rỗng", nameof(paymentData.description));
                }

                if (paymentData.items == null || !paymentData.items.Any())
                {
                    throw new ArgumentException("Items không được null hoặc rỗng", nameof(paymentData.items));
                }

                // Gọi PayOS SDK để tạo payment link
                var result = await _payOS.createPaymentLink(paymentData);

                _logger.LogInformation("Tạo thành công PayOS payment link cho OrderCode: {OrderCode}. CheckoutUrl: {CheckoutUrl}",
                    paymentData.orderCode, result.checkoutUrl);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo PayOS payment link cho OrderCode: {OrderCode}",
                    paymentData?.orderCode);

                if (ex is ArgumentNullException || ex is ArgumentException)
                {
                    throw; // Re-throw validation errors
                }

                throw new ErrorException($"Không thể tạo payment link PayOS: {ex.Message}");
            }
        }

        
        public async Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode)
        {
            try
            {
                _logger.LogInformation("Lấy thông tin PayOS payment link cho OrderCode: {OrderCode}", orderCode);

                // Validate input
                if (orderCode <= 0)
                {
                    throw new ArgumentException("OrderCode phải là số dương", nameof(orderCode));
                }

                // Gọi PayOS SDK để lấy thông tin payment link
                var paymentLinkInfo = await _payOS.getPaymentLinkInformation(orderCode);

                _logger.LogInformation("Lấy thành công thông tin PayOS payment link cho OrderCode: {OrderCode}. Status: {Status}",
                    orderCode, paymentLinkInfo.status);

                return paymentLinkInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin PayOS payment link cho OrderCode: {OrderCode}", orderCode);

                if (ex is ArgumentException)
                {
                    throw; // Re-throw validation errors
                }

                throw new ErrorException($"Không thể lấy thông tin payment link PayOS cho OrderCode {orderCode}: {ex.Message}");
            }
        }

        
        public async Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode, string cancellationReason)
        {
            try
            {
                _logger.LogInformation("Hủy PayOS payment link cho OrderCode: {OrderCode}, Lý do: {Reason}",
                    orderCode, cancellationReason);

                // Validate input
                if (orderCode <= 0)
                {
                    throw new ArgumentException("OrderCode phải là số dương", nameof(orderCode));
                }

                if (string.IsNullOrWhiteSpace(cancellationReason))
                {
                    cancellationReason = "Hủy thanh toán"; // Default reason
                }

                // Gọi PayOS SDK để hủy payment link
                var cancelledPaymentInfo = await _payOS.cancelPaymentLink(orderCode, cancellationReason);

                _logger.LogInformation("Hủy thành công PayOS payment link cho OrderCode: {OrderCode}. Status: {Status}",
                    orderCode, cancelledPaymentInfo.status);

                return cancelledPaymentInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy PayOS payment link cho OrderCode: {OrderCode}", orderCode);

                if (ex is ArgumentException)
                {
                    throw; // Re-throw validation errors
                }

                throw new ErrorException($"Không thể hủy payment link PayOS cho OrderCode {orderCode}: {ex.Message}");
            }
        }

        
        public async Task<string> ConfirmWebhookAsync(string webhookUrl)
        {
            try
            {
                _logger.LogInformation("Xác thực và đăng ký PayOS webhook URL: {WebhookUrl}", webhookUrl);

                // Validate input
                if (string.IsNullOrWhiteSpace(webhookUrl))
                {
                    throw new ArgumentException("Webhook URL không được null hoặc rỗng", nameof(webhookUrl));
                }

                // Validate URL format
                if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uriResult)
                    || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ArgumentException("Webhook URL không hợp lệ", nameof(webhookUrl));
                }

                // Gọi PayOS SDK để xác thực và đăng ký webhook
                var confirmResult = await _payOS.confirmWebhook(webhookUrl);

                _logger.LogInformation("Xác thực thành công PayOS webhook URL: {WebhookUrl}. Result: {Result}",
                    webhookUrl, confirmResult);

                return confirmResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực PayOS webhook URL: {WebhookUrl}", webhookUrl);

                if (ex is ArgumentException)
                {
                    throw; // Re-throw validation errors
                }

                throw new ErrorException($"Không thể xác thực webhook URL PayOS: {ex.Message}");
            }
        }

        
        public WebhookData VerifyPaymentWebhookData(WebhookType webhookType)
        {
            try
            {
                _logger.LogInformation("Xác minh PayOS webhook data. Code: {Code}", webhookType?.code);

                // Validate input
                if (webhookType == null)
                {
                    throw new ArgumentNullException(nameof(webhookType), "WebhookType không được null");
                }

                if (string.IsNullOrWhiteSpace(webhookType.signature))
                {
                    throw new ArgumentException("Webhook signature không được rỗng", nameof(webhookType.signature));
                }

                // Gọi PayOS SDK để xác minh webhook data
                var verifiedData = _payOS.verifyPaymentWebhookData(webhookType);

                _logger.LogInformation("Xác minh thành công PayOS webhook data. OrderCode: {OrderCode}, Amount: {Amount}",
                    verifiedData.orderCode, verifiedData.amount);

                return verifiedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác minh PayOS webhook data");

                if (ex is ArgumentNullException || ex is ArgumentException)
                {
                    throw; // Re-throw validation errors
                }

                throw new ErrorException($"Không thể xác minh webhook data PayOS: {ex.Message}");
            }
        }

        #region Additional Helper Methods

        
        private bool IsValidPaymentData(PaymentData paymentData)
        {
            if (paymentData == null) return false;
            if (paymentData.orderCode <= 0) return false;
            if (paymentData.amount <= 0) return false;
            if (string.IsNullOrWhiteSpace(paymentData.description)) return false;
            if (paymentData.items == null || !paymentData.items.Any()) return false;
            if (string.IsNullOrWhiteSpace(paymentData.returnUrl)) return false;
            if (string.IsNullOrWhiteSpace(paymentData.cancelUrl)) return false;

            return true;
        }

        
        public static long GenerateUniqueOrderCode(Guid guid)
        {
            long orderCode = Math.Abs(guid.GetHashCode()) * 10000 + (DateTime.Now.Hour * 100 + DateTime.Now.Minute);
            if (orderCode < 0)
                return -orderCode;
            return orderCode;
        }

        
        public static int GenerateShortOrderCode()
        {
            return int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        }

        #endregion
    }
}