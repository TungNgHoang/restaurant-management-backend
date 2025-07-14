namespace RestaurantManagement.Service.Dtos.PromotionDto
{
    public class PromotionDto
    {
        public Guid ProId { get; set; }
        public string ProCode { get; set; } = null!;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = null!;
        public decimal DiscountVal { get; set; }
        public decimal? ConditionVal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProQuantity { get; set; }
    }
}