using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblAttendance
{
    public Guid AttendanceId { get; set; }

    public Guid StaId { get; set; }

    public Guid ShiftId { get; set; }

    public DateOnly WorkDate { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public string? Status { get; set; }
}
