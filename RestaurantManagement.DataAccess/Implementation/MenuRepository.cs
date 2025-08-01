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
    public class MenuRepository : Repository<TblMenu>, IMenuRepository
    {
        private readonly RestaurantDBContext _context;

        public MenuRepository(RestaurantDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TblMenu>> GetAllMenuAsync()
        {
            return await ActiveRecordsAsync(); // Lấy danh sách món có trạng thái Active
        }


    }
}
