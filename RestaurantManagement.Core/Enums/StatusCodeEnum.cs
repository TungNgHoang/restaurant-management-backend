using System.ComponentModel;

namespace RestaurantManagement.Core.Enums
{
    public enum StatusCodeEnum
    {
        Success = 0,

        [Description("System Error.")]
        Error = 1,

        [Description("Reservation not found.")]
        ReservatioNotFound = 2,

        [Description("Not Found")]
        PageIndexInvalid,

        [Description("Page Size Invalid")]
        PageSizeInvalid = 4,

        [Description("Trạng thái reservation không hợp lệ để check-in.")]
        A01,

        [Description("Username not found. Please try again.")]
        B01,

        [Description("Incorrect password. Please try again.")]
        B02,

        [Description("Bàn không khả dụng để check-in.")]
        A02,

        [Description("Reservation không tồn tại hoặc không ở trạng thái Serving.")]
        A03,

        [Description("Bàn không tồn tại hoặc không ở trạng thái Occupied.")]
        A04,
        [Description("Bàn đã được huỷ thành công.")]
        A05,

        [Description("Đặt bàn thành công.")]
        A06,


        [Description("Bàn đã được đặt trong khoảng thời gian này.")]
        C01,

        [Description("Thời gian đặt bàn không hợp lệ.")]
        C02,

        [Description("Số điện thoại không hợp lệ.")]
        C03,

        [Description("Id bàn không hợp lệ.")]
        C04,

        [Description("Download Interrupted. Please check your internet connection and try again.")]
        C05,

        //This message is used in BE, not display to FE screen 
        [Description("At least one creation type must be selected is CREATE EXCEPTIONS.")]
        C06,

        [Description("Không tìm thấy Reservation hợp lệ")]
        C07,

        [Description("Reservation đã order rồi, ko thể preorder")]
        C08,

        [Description("Không tìm thấy thông tin khách")]
        C09,

        //StatusCode cho Order
        [Description("Món ăn không tồn tại")]
        D01,

        [Description("Không tìm thấy chi tiết đơn hàng với OrdId chỉ định")]
        D02,

        [Description("Xoá món ăn thành công!")]
        D03,

        [Description("Xoá đơn hàng thành công!")]
        D06,
        [Description("Không được để trống ID đơn hàng")]
        D11,

        //StatusCode cho Promotion
        [Description("Không tìm thấy khuyến mãi")]
        D04,
        [Description("Xóa Voucher thành công")]
        D05,
        [Description("Khuyến mãi không được áp dụng cho hạng khách hàng này.")]
        D07,
        [Description("Khuyến mãi không được áp dụng do không đủ điều kiện.")]
        D08,
        [Description("Khuyến mãi đã hết số lượng.")]
        D09,
        [Description("Mã khuyến mãi đã tồn tại")]
        D10,
        //Validation
        [Description("Tên món không được để trống")]
        V01,
        //StatusCode cho Staff
        [Description("Không tìm thấy nhân viên với Id chỉ định")]
        E01,
        [Description("Xoá nhân viên thành công!")]
        E02,
        [Description("Không tìm thấy tài khoản người dùng với Id chỉ định")]
        E03,
        [Description("Lương cơ bản của nhân viên phải lớn hơn 0")]
        E04,
        [Description("Số điện thoại của nhân viên không hợp lệ")]
        E05,
        [Description("Mật khẩu cũ không đúng")]
        E06,
        [Description("Mật khẩu mới và xác nhận mật khẩu không khớp")]
        E07,
        [Description("Email đã tồn tại")]
        E08,

        //StatusCode cho Notification
        [Description("Gửi thông báo thành công")]
        F01,
        [Description("Không tìm thấy thông báo")]
        F02,
        [Description("Đánh dấu đã đọc thông báo thành công")]
        F03,

        //StatusCode cho Assignment
        [Description("Không tìm thấy phân công trong thời gian quy định")]
        G01,
        [Description("Tạo phân công thành công")]
        G02,
        [Description("Nhân viên này không tồn tại để phân công")]
        G03,
        [Description("Cập nhật phân công thành công")]
        G04,


        [Description("Bad request.")]
        BadRequest,

        [Description("Invalid filter option.")]
        InvalidOption,

        [Description("Unmatched columns found.")]
        UnmatchedColumns,

        [Description("Logout failed")]
        LogoutFailed,
    }
}
