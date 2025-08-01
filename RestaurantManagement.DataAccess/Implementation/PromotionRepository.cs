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
    public class PromotionRepository : Repository<TblPromotion>, IPromotionRepository
    {
        private readonly RestaurantDBContext _context;
        public PromotionRepository(RestaurantDBContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TblPromotion>> GetAllPromotionsAsync()
        {
            return await ActiveRecordsAsync();
        }
    }
}