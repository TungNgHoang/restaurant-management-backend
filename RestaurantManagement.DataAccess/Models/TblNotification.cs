namespace RestaurantManagement.DataAccess.Models;

public partial class TblNotification
{
    public Guid NotiId { get; set; }

    public Guid? ResId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblReservation? Res { get; set; }
}
