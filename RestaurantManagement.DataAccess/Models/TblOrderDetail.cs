﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RestaurantManagement.Api.Models;

public partial class TblOrderDetail
{
    public Guid OdtId { get; set; }

    public Guid OrdId { get; set; }

    public Guid MnuId { get; set; }

    public int OdtQuantity { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; }

    public virtual TblMenu Mnu { get; set; }

    public virtual TblOrderInfo Ord { get; set; }
}