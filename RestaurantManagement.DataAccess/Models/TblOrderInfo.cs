using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblOrderInfo
{
    public Guid OrdId { get; set; }

    public Guid CusId { get; set; }

    public Guid TbiId { get; set; }

    public decimal TotalPrice { get; set; }

    public Guid? ResId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblCustomer Cus { get; set; } = null!;

    public virtual TblReservation? Res { get; set; }

    public virtual TblTableInfo Tbi { get; set; } = null!;

    public virtual ICollection<TblOrderDetail> TblOrderDetails { get; set; } = new List<TblOrderDetail>();

    public virtual ICollection<TblPayment> TblPayments { get; set; } = new List<TblPayment>();
}
