using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblTableInfo
{
    public Guid TbiId { get; set; }

    public string? TbiQrcode { get; set; }

    public string? TbiStatus { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public int TbiTableNumber { get; set; }

    public virtual ICollection<TblOrderInfo> TblOrderInfos { get; set; } = new List<TblOrderInfo>();

    public virtual ICollection<TblReservation> TblReservations { get; set; } = new List<TblReservation>();
}
