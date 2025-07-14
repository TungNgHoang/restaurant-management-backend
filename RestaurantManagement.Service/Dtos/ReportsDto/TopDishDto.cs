namespace RestaurantManagement.Service.Dtos.ReportsDto
{
    public class TopDishDto
    {
        public Guid MnuId { get; set; }
        public string MnuName { get; set; }
        public int QuantitySold { get; set; }
        public decimal GrowthPercentage { get; set; }
        public string MnuImage { get; set; }
        public decimal MnuPrice { get; set; }
    }
}
