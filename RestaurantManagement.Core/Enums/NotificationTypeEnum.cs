using System.ComponentModel;

namespace RestaurantManagement.Core.Enums
{
    public enum NotificationTypeEnum
    {
        [Description("Đặt bàn mới")]
        NewReservation = 1,

        [Description("Thanh toán thành công")]
        PaymentSuccess = 2,

        [Description("Đơn đặt bàn sắp quá hạn")]
        ReservationExpiringSoon = 3,

        [Description("Báo cáo doanh thu tháng")]
        MonthlyRevenueReport = 4,

        [Description("Đơn đặt bàn đã bị hủy tự động")]
        ReservationAutoCancelled = 5,

        [Description("Đơn đặt trước đã bị hủy tự động")]
        PreOrderAutoCancelled = 6
    }
}
