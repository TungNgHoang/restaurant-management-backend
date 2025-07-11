namespace RestaurantManagement.Service.Dtos.OrdersDto
{
    public class OrderDetailsDto
    {
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public Guid MnuId { get; set; }
        public string MnuName { get; set; }
        public string MnuImage { get; set; }
        public decimal MnuPrice { get; set; }
        public int OdtQuantity { get; set; }
    }
}
