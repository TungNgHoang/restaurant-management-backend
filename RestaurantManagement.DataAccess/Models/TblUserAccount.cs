using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblUserAccount
{
    public Guid UacId { get; set; }

    public string UacEmail { get; set; } = null!;

    public string UacPassword { get; set; } = null!;

    public string UacRole { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<TblBlackListToken> TblBlackListTokens { get; set; } = new List<TblBlackListToken>();

    public virtual ICollection<TblStaff> TblStaffs { get; set; } = new List<TblStaff>();
}
