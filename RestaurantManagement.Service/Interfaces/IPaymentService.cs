using Elastic.Clients.Elasticsearch.Inference;
using Net.payOS.Types;
using RestaurantManagement.Service.Dtos.PaymentDto;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<byte[]> CheckoutAndPayAsync(Guid resId, Guid ordId, string proCode, string payMethod);
        //Task<PayOSPaymentResponseDto> CreatePayOSPaymentAsync(Guid resId, Guid ordId, string? proCode);
        //Task HandlePayOSWebhookAsync(WebhookType webhookData);
    }
}
