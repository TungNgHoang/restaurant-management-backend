﻿namespace RestaurantManagement.Service.Dtos.ReportsDto
{
    public class DashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public int TotalDishesSold { get; set; }
        public decimal DishesChangePercentage { get; set; }
        public int TotalReservations { get; set; }
        public decimal ReservationsChangePercentage { get; set; }
        public int TotalCustomers { get; set; }
        public decimal CustomersChangePercentage { get; set; }
    }
}
