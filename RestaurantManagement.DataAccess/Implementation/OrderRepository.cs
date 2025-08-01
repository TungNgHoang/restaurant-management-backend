﻿using Microsoft.EntityFrameworkCore;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly RestaurantDBContext _context;

        public OrderRepository(RestaurantDBContext context)
        {
            _context = context;
        }
        public async Task<TblOrderInfo> CreateOrderAsync(TblOrderInfo order, List<TblOrderDetail> orderDetails)
        {
            _context.TblOrderInfos.Add(order);
            await _context.SaveChangesAsync();

            foreach (var detail in orderDetails)
            {
                detail.OrdId = order.OrdId;
                _context.TblOrderDetails.Add(detail);
            }
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<TblOrderInfo> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.TblOrderInfos
                .Include(o => o.TblOrderDetails)
                .ThenInclude(d => d.Mnu)
                .FirstOrDefaultAsync(o => o.OrdId == orderId);
        }
    }
}
