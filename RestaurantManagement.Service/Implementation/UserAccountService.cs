using AutoMapper;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Dtos.AuthDto;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RestaurantManagement.Service.Implementation
{
    public class UserAccountService : BaseService, IUserAccountService
    {
        private readonly IRepository<TblUserAccount> _userAccountRepository;
        private readonly IAuthService _authService;
        private new readonly IMapper _mapper;

        public UserAccountService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblUserAccount> userAccountRepository,
            IAuthService authService)
            : base(appSettings, mapper)
        {
            _mapper = mapper;
            _userAccountRepository = userAccountRepository;
            _authService = authService;
        }

        public Task<UserAccountDto> CreateUserAccountAsync(UserAccountDto userAccountDto)
        {
            throw new NotImplementedException();
        }

        public Task<UserAccountDto> GetUserAccountByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> LoginAsync(LoginRequestDto loginRequest)
        {
            // 1. Tìm user trong DB dựa trên Email
            var user = await _userAccountRepository.FindAsync(u => u.UacEmail== loginRequest.Email);
            if (user == null)
            {
                //throw new UnauthorizedAccessException("User not found.");
                throw new ErrorException(Core.Enums.StatusCodeEnum.B01);
            }

            var passwordHasher = new PasswordHasher<TblUserAccount>();
            var result = passwordHasher.VerifyHashedPassword(user, user.UacPassword, loginRequest.Password);

            if (result != PasswordVerificationResult.Failed)
            {
                //throw new UnauthorizedAccessException("Invalid password.");
                throw new ErrorException(Core.Enums.StatusCodeEnum.B01);
            }
            // 3. Gọi AuthService để tạo token
            //    Role được lấy từ DB: user.UacRole
            var token = await _authService.GenerateJwtTokenAsync(user);
            return token;
        }
    }
}
