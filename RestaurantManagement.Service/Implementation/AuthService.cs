using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantManagement.Api.Models;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Service.Interfaces;
using System;
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

        public AuthService(AppSettings appSettings, IMapper mapper, IConfiguration configuration) : base(appSettings, mapper)
        {
            _configuration = configuration;
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
    }
}
