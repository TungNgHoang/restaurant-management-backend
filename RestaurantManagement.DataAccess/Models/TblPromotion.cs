using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblPromotion
{
    public Guid ProId { get; set; }

    public string ProCode { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountVal { get; set; }

    public decimal? ConditionVal { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public int ProQuantity { get; set; }

    public virtual ICollection<TblOrderInfo> Ords { get; set; } = new List<TblOrderInfo>();
}
