namespace RestaurantManagement.Service.Dtos.StaffDto
{
    public class GetStaffByIdDto
    {
        public Guid StaId { get; set; }
        public Guid UacId { get; set; }
        public string StaName { get; set; }
        public string StaRole { get; set; }
        public string StaEmail { get; set; }
        public string StaPhone { get; set; }
        public decimal StaBaseSalary { get; set; }
    }
}
