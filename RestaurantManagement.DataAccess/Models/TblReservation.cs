﻿using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblReservation
{
    public Guid ResId { get; set; }

    public Guid? CusId { get; set; }

    public Guid TbiId { get; set; }

    public DateTime ResDate { get; set; }

    public int ResNumber { get; set; }

    public string? ResStatus { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public DateTime? ResEndTime { get; set; }

    public string? Note { get; set; }

    public string? TempCustomerName { get; set; }

    public string? TempCustomerPhone { get; set; }

    public int? ResActualNumber { get; set; }

    public DateTime? ResAutoCancelAt { get; set; }

    public string? TempCustomerMail { get; set; }

    public virtual TblCustomer? Cus { get; set; }

    public virtual TblTableInfo Tbi { get; set; } = null!;

    public virtual ICollection<TblNotification> TblNotifications { get; set; } = new List<TblNotification>();

    public virtual ICollection<TblOrderInfo> TblOrderInfos { get; set; } = new List<TblOrderInfo>();
}
