﻿namespace RestaurantManagement.Service.Dtos.ReserDto
{
    public class ReservationResponseDto
    {
        public Guid ResId { get; set; }
        public Guid CusId { get; set; }
        public Guid TbiId { get; set; }
        public DateTime ResDate { get; set; }
        public DateTime? ResEndTime { get; set; }
        public int ResNumber { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
