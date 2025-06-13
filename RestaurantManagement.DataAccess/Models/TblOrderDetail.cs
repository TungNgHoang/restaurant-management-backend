using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

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

    public byte[] RowVersion { get; set; } = null!;

    public string OdtStatus { get; set; } = null!;

    public virtual TblMenu Mnu { get; set; } = null!;

    public virtual TblOrderInfo Ord { get; set; } = null!;
}
