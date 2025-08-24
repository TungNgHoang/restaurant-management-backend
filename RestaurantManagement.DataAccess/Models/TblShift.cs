using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblShift
{
    public Guid ShiftId { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool? IsDeleted { get; set; }
}
