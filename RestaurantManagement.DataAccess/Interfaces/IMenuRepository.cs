using RestaurantManagement.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IMenuRepository : IRepository<TblMenu>
    {
        Task<IEnumerable<TblMenu>> GetAllMenuAsync();   
    }
}
