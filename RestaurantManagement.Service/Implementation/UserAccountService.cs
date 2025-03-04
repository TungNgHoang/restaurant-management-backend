using AutoMapper;
using RestaurantManagement.Api.Models;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Dtos;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Tìm user theo email (được lưu trong trường UacUsername)
            var user = await _userAccountRepository.FindAsync(u => u.UacEmail == loginRequest.Email);
            if (user == null)
            {
                // Không tìm thấy user
                return null;
            }

            // Kiểm tra password (so sánh trực tiếp, cần hash hoặc bảo mật thêm trong thực tế)
            if (user.UacPassword != loginRequest.Password)
            {
                // Mật khẩu không đúng
                throw new UnauthorizedAccessException("Sai mật khẩu.");
            }

            // Tạo JWT token qua AuthService
            var token = await _authService.GenerateJwtTokenAsync(user);
            return token;
        }
    }
}
