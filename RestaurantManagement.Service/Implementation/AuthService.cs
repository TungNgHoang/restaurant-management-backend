using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Dtos;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly RestaurantDBContext _dbContext;
        private readonly IRepository<TblBlackListToken> _blackListTokenRepository;

        public AuthService(
            AppSettings appSettings,
            IMapper mapper,
            IConfiguration configuration,
            IRepository<TblBlackListToken> blackListTokenRepository,
            RestaurantDBContext dbContext) : base(appSettings, mapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _blackListTokenRepository = blackListTokenRepository ?? throw new ArgumentNullException(nameof(blackListTokenRepository));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<string> GenerateJwtTokenAsync(TblUserAccount user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(user.UacEmail) || string.IsNullOrEmpty(user.UacRole))
                throw new ArgumentException("Email hoặc vai trò của người dùng không hợp lệ.");

            var jwtSettings = _configuration.GetSection("AppSettings:Jwt");
            var keyValue = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiresTime = jwtSettings["AccessTokenExpiresTime"];

            if (string.IsNullOrEmpty(keyValue) || string.IsNullOrEmpty(issuer) ||
                string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(expiresTime))
                throw new InvalidOperationException("Cấu hình JWT không hợp lệ.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

            // Tạo danh sách claims, bao gồm thông tin username, role và JTI
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UacEmail),
                new Claim(ClaimTypes.Role, user.UacRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Thêm JTI để định danh token
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(expiresTime)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        // Lưu token vào Blacklist khi logout
        public async Task<bool> LogoutAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            // Lấy ngày hết hạn của token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return false;

            var expiryDate = jwtToken.ValidTo;

            // Lưu token vào database
            var blacklistedToken = new TblBlackListToken
            {
                Token = token,
                ExpiryDate = expiryDate
            };

            await _blackListTokenRepository.InsertAsync(blacklistedToken);
            await _blackListTokenRepository.SaveChangesAsync(); // Lưu thay đổi vào DB
            return true;
        }

        // Fix for the CS1061 error in the IsTokenBlacklisted method
        public async Task<bool> IsTokenBlacklisted(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            // Kiểm tra token có hết hạn chưa
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null || jwtToken.ValidTo < DateTime.UtcNow)
                return true; // Token không hợp lệ hoặc đã hết hạn

            // Sửa lỗi: Ép kiểu _blackListTokenRepository.GetAll() về IQueryable<TblBlackListToken>
            var isBlacklisted = await ((IQueryable<TblBlackListToken>)_blackListTokenRepository.GetAll())
                .AnyAsync(t => t.Token == token);
            return isBlacklisted;
        }
    }
}