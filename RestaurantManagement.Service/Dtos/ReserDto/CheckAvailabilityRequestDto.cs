﻿namespace RestaurantManagement.Service.Dtos.ReserDto
{
    public class CheckAvailabilityRequestDto
    {
        public DateTime ResDate { get; set; } // Ngày và giờ đến
        public DateTime? ResEndDate { get; set; } // Khoảng thời gian ở lại
        public int ResNumber { get; set; }
    }
}
