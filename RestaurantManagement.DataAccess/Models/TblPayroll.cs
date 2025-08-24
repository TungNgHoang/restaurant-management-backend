using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblPayroll
{
    public Guid PayrollId { get; set; }

    public Guid StaId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal TotalHours { get; set; }

    public decimal TotalSalary { get; set; }

    public DateTime? CreatedAt { get; set; }
}
