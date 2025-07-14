namespace RestaurantManagement.Service.Dtos.ReportsDto
{
    public class InvoiceDto
    {
        public int TableNumber { get; set; }       // Số bàn từ Table.TbiTableNumber
        public string CustomerName { get; set; }   // Tên khách từ Reservation.TempCustomerName
        public string CustomerPhone { get; set; }  // Số điện thoại khách hàng lấy từ Reservation.TempCustomerPhone
        public DateTime Date { get; set; }         // Ngày đặt bàn từ Reservation.ResDate
        public TimeSpan TimeIn { get; set; }       // Giờ vào từ Reservation.ResDate
        public TimeSpan TimeOut { get; set; }      // Giờ ra từ Reservation.ResEndTime
        public int People { get; set; }            // Số người từ Reservation.ResNumber
        public decimal TotalPrice { get; set; }    // Tổng tiền từ OrderInfo.TotalPrice
        public string PayMethod { get; set; }      // Phương thức thanh toán từ Payment.PayMethod
    }
}
