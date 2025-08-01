﻿using RestaurantManagement.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IOrderRepository
    {
        Task<TblOrderInfo> CreateOrderAsync(TblOrderInfo order, List<TblOrderDetail> orderDetails);
        Task<TblOrderInfo> GetOrderByIdAsync(Guid orderId);
    }
}
