namespace RestaurantManagement.Service.Dtos.ReserDto
{
    public class CreateReservationRequestDto
    {
        public string TempCustomerName { get; set; }
        public string TempCustomerPhone { get; set; }
        public string TempCustomerEmail { get; set; }
        public Guid TbiId { get; set; }
        public DateTime ResDate { get; set; } // Ngày và giờ đến
        public DateTime ResEndTime { get; set; } // Khoảng thời gian ở lại
        public int ResNumber { get; set; }
        public string Note { get; set; }
    }
}
