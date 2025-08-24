using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RestaurantManagement.Service.Dtos.PaymentDto
{
    public class PaymentLinkResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public PaymentLinkData? Data { get; set; }
    }

    public class PaymentLinkData
    {
        public string AccountNumber { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public long OrderCode { get; set; }
        public string Currency { get; set; } = "VND";
        public string PaymentLinkId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
    }
}
