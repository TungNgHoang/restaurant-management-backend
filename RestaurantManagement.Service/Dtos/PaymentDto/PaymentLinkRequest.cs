using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RestaurantManagement.Service.Dtos.PaymentDto
{
    public class PaymentLinkRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ReservationId { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public List<PaymentItem> Items { get; set; } = new List<PaymentItem>();

        public string? BuyerName { get; set; }

        public string? BuyerEmail { get; set; }

        public string? BuyerPhone { get; set; }

        public string? ReturnUrl { get; set; }

        public string? CancelUrl { get; set; }
    }

    public class PaymentItem
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int Price { get; set; }
    }
}
