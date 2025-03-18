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

        [Description("Concurrency Conflict")]
        ConcurrencyConflict = 2,

        [Description("Not Found")]
        PageIndexInvalid = 3,

        [Description("Page Size Invalid")]
        PageSizeInvalid = 4,

        [Description("{Required Field} is required.")]
        A01,

        [Description("Incorrect username or password. Please try again.")]
        B01,

        [Description("{{Object}} not found")]
        A02,

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

        //StatusCode cho Order
        [Description("Món ăn không tồn tại")]
        D01,

        [Description("Không tìm thấy chi tiết đơn hàng với OrdId chỉ định")]
        D02,

        //This message is used in BE, not display to FE screen 
        [Description("At least one creation type must be selected is CREATE EXCEPTIONS.")]
        C06,

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
