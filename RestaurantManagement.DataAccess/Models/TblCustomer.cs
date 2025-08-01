﻿using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblCustomer
{
    public Guid CusId { get; set; }

    public string CusName { get; set; } = null!;

    public string? CusContact { get; set; }

    public string? CusEmail { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public int CusPoints { get; set; }

    public string? CusTier { get; set; }

    public virtual ICollection<TblOrderInfo> TblOrderInfos { get; set; } = new List<TblOrderInfo>();

    public virtual ICollection<TblPayment> TblPayments { get; set; } = new List<TblPayment>();

    public virtual ICollection<TblReservation> TblReservations { get; set; } = new List<TblReservation>();
}
