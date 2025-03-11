using System;
using System.Collections.Generic;

namespace RestaurantManagement.Api.Models;

public partial class TblBlackListToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }
}
