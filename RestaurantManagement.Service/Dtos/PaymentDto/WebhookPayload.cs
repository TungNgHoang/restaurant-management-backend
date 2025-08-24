using Newtonsoft.Json;

namespace RestaurantManagement.Service.Dtos.PaymentDto
{
    public class WebhookPayload
    {
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public PayOSWebhookData Data { get; set; } = new PayOSWebhookData();
        public string Signature { get; set; } = string.Empty;
    }
    public class PayOSWebhookData
    {
        public int OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string TransactionDateTime { get; set; } = string.Empty;
        public string Currency { get; set; } = "VND";
        public string PaymentLinkId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string CounterAccountBankId { get; set; } = string.Empty;
        public string CounterAccountBankName { get; set; } = string.Empty;
        public string CounterAccountName { get; set; } = string.Empty;
        public string CounterAccountNumber { get; set; } = string.Empty;
        public string VirtualAccountName { get; set; } = string.Empty;
        public string VirtualAccountNumber { get; set; } = string.Empty;
    }
}
