using System;
using System.Collections.Generic;

namespace RestaurantManagement.Api.Models;

public partial class TblStaff
{
    public Guid StaId { get; set; }

    public string StaName { get; set; } = null!;

    public string? StaRole { get; set; }

    public Guid UacId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblUserAccount Uac { get; set; } = null!;
}
