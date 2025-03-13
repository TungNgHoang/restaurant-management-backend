using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.AuthDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IUserAccountService 
    {
        Task<UserAccountDto> CreateUserAccountAsync(UserAccountDto userAccountDto);
        Task<UserAccountDto> GetUserAccountByIdAsync(Guid id);
        Task<string> LoginAsync(LoginRequestDto loginRequest);
    }
}
