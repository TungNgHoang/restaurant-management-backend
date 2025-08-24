namespace RestaurantManagement.Service.Dtos.PaymentDto
{
    public class PayOSOrderStatusDto
    {
        public Guid OrderId { get; set; }
        public long OrderCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionId { get; set; }
        public string PaymentMethod { get; set; } = "PayOS";
        public string Description { get; set; }
    }
}
