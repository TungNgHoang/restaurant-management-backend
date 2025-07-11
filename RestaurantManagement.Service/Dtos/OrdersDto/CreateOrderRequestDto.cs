namespace RestaurantManagement.Service.Dtos.OrdersDto
{
    public class CreateOrderRequestDto
    {
        public Guid CusId { get; set; }
        public Guid TbiId { get; set; }
        public Guid? ResId { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class OrderDetailDto
    {
        public Guid MnuId { get; set; }  // Mã món ăn
        public int OdtQuantity { get; set; }  // Số lượng món
    }
}
