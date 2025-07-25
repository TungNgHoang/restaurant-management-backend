using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        PageIndexInvalid = 3,

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

        //StatusCode cho Order
        [Description("Món ăn không tồn tại")]
        D01,

        [Description("Không tìm thấy chi tiết đơn hàng với OrdId chỉ định")]
        D02,

        [Description("Xoá món ăn thành công!")]
        D03,

        [Description("Xoá đơn hàng thành công!")]
        D06,

        //StatusCode cho Promotion
        [Description("Không tìm thấy khuyến mãi")]
        D04,
        [Description("Xóa Voucher thành công")]
        D05,
        [Description("Mã khuyến mãi đã tồn tại")]
        D07,
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
