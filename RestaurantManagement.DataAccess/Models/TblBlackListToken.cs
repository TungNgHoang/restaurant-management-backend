using System;
using System.Collections.Generic;

namespace RestaurantManagement.DataAccess.Models;

public partial class TblBlackListToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public Guid? UacId { get; set; }

    public DateTime RevokedAt { get; set; }

    public virtual TblUserAccount? Uac { get; set; }
}
