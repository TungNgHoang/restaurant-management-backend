using Net.payOS.Types;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IPayOSService
    {
        Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData);

        Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode);

        Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode, string cancellationReason);

        Task<string> ConfirmWebhookAsync(string webhookUrl);

        WebhookData VerifyPaymentWebhookData(WebhookType webhookType);
    }
}
