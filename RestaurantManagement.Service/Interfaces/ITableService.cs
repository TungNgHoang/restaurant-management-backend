﻿namespace RestaurantManagement.Service.Interfaces
{
    public interface ITableService
    {
        Task<IEnumerable<TableDto>> GetAllTableAsync(TableModels pagingModel);
    }
}
