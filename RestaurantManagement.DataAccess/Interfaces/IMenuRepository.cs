﻿using RestaurantManagement.DataAccess.Models;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IMenuRepository : IRepository<TblMenu>
    {
        Task<IEnumerable<TblMenu>> GetAllMenuAsync();
    }
}
