namespace RestaurantManagement.Service.Dtos.ReserDto
{
    public class ReserDto
    {
        public Guid ResId { get; set; }            // Id từ Reservation.ResId
        public Guid? OrdId { get; set; }             // Lấy OrderID
        public int TableNumber { get; set; }       // Số bàn từ Table.TbiTableNumber
        public string CustomerMail { get; set; }    
        public string CustomerName { get; set; }   // Tên khách từ Reservation.TempCustomerName
        public string ContactPhone { get; set; }   // Số điện thoại từ Reservation.TempCustomerPhone
        public DateTime ReservationDate { get; set; } // Ngày đến từ ResDate
        public TimeSpan TimeIn { get; set; }       // Giờ đến từ ResDate
        public TimeSpan TimeOut { get; set; }      // Giờ đi từ ResEndTime
        public int NumberOfPeople { get; set; }    // Số người từ Reservation.ResNumber
        public string Status { get; set; }         // Trạng thái từ Reservation.ResStatus
    }
}
