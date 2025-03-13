using RestaurantManagement.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IAuthService
    {
        // Nhận đối tượng TblUserAccount và trả về JWT token dạng string
        Task<string> GenerateJwtTokenAsync(TblUserAccount user);
        Task<bool> LogoutAsync(string token); // Thêm dòng này để khai báo LogoutAsync

    }
}
