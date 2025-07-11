namespace RestaurantManagement.Service.Dtos.ReserDto
{
    public class AvailableTableDto
    {
        public Guid TbiId { get; set; }
        public int TbiTableNumber { get; set; }
        public int TbiCapacity { get; set; }
    }
}
