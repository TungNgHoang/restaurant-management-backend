﻿namespace RestaurantManagement.Service.ApiModels
{
    public class StatisticsRequest
    {
        public string Period { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
    }
}
