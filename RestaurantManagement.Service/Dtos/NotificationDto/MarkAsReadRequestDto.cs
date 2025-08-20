namespace RestaurantManagement.Service.Dtos.NotificationDto
{
    public class MarkAsReadRequestDto
    {
        public List<Guid> NotificationIds { get; set; } = new();
    }
}
