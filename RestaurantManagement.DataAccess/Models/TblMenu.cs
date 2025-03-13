using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblMenu
{
    public Guid MnuId { get; set; }

    public string MnuName { get; set; } = null!;

    public decimal MnuPrice { get; set; }

    public string? MnuStatus { get; set; }

    public string? MnuImage { get; set; }

    public string? MnuDescription { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<TblOrderDetail> TblOrderDetails { get; set; } = new List<TblOrderDetail>();
}
