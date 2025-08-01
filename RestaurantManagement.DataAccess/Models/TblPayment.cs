﻿using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblPayment
{
    public Guid PayId { get; set; }

    public Guid OrdId { get; set; }

    public Guid CusId { get; set; }

    public decimal Amount { get; set; }

    public string? PayMethod { get; set; }

    public string? PayStatus { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblCustomer Cus { get; set; } = null!;

    public virtual TblOrderInfo Ord { get; set; } = null!;
}
