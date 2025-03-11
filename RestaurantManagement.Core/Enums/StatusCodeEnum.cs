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

        [Description("Stale Dated Days must be from 60 to 365.")]
        C01,

        [Description("This Client Number is already active in the system.")]
        C02,

        [Description("This Account Number is already active in the system.")]
        C03,

        [Description("{{Client_Name}} has been created successfully.")]
        C04,

        [Description("Download Interrupted. Please check your internet connection and try again.")]
        C05,

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
