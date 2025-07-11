namespace RestaurantManagement.Service.Dtos.OrdersDto
{
    public class OrderDTO
    {
        public Guid OrdID { get; set; }
        public Guid CusID { get; set; }
        public Guid TbiID { get; set; }
        public Guid ResID { get; set; }
        public OrderStatusEnum OrdStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; }
    }

    public class OrderDetailDTO
    {
        public Guid OdtID { get; set; }
        public Guid MnuID { get; set; }
        public int OdtQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
