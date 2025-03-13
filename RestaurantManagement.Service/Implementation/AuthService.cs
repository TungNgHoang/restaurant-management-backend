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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Twilio.Jwt.AccessToken;

namespace RestaurantManagement.Service.Implementation
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly RestaurantDBContext _dbContext;
        private readonly IRepository<TblBlackListToken> _blackListTokenRepository;


        public AuthService(AppSettings appSettings, IMapper mapper, IConfiguration configuration, IRepository<TblBlackListToken> blackListTokenRepository) : base(appSettings, mapper)
        {
            _configuration = configuration;
            _blackListTokenRepository = blackListTokenRepository;
        }

        public Task<string> GenerateJwtTokenAsync(TblUserAccount user)
        {
            var jwtSettings = _configuration.GetSection("AppSettings:Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            // Tạo danh sách claims, bao gồm thông tin username và role
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UacEmail),
            new Claim(ClaimTypes.Role, user.UacRole)
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["AccessTokenExpiresTime"])),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        // Phương thức logout: lưu token vào bảng BlacklistedTokens trong DB
        // Lưu token vào Blacklist khi logout
        public async Task<bool> LogoutAsync(string token)
        {
            //if (string.IsNullOrEmpty(token))
               // return false;

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
            //await _dbContext.SaveChangesAsync();
            return true;
        }

        // Kiểm tra token có trong blacklist không
        public async Task<bool> IsTokenBlacklisted(string token)
        {
            return await _dbContext.TblBlackListTokens.AnyAsync(t => t.Token == token);
        }

        
    }
}
