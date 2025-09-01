using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.AuthDto;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.UnitTest.Services
{
    public class UserAccountServiceTests
    {
        private readonly Mock<IRepository<TblUserAccount>> _userRepo;
        private readonly Mock<IAuthService> _authService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<RestaurantDBContext> _dbContext;
        private readonly AppSettings _appSettings;

        private readonly UserAccountService _sut;

        public UserAccountServiceTests()
        {
            _userRepo = new Mock<IRepository<TblUserAccount>>();
            _authService = new Mock<IAuthService>();
            _mapper = new Mock<IMapper>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _dbContext = new Mock<RestaurantDBContext>(); // nếu sealed thì thay bằng InMemoryDbContext
            _appSettings = new AppSettings
            {
                // setup giá trị cần thiết nếu có (ví dụ JWTSecret, Expiry, v.v.)
            };

            _sut = new UserAccountService(
                _appSettings,
                _mapper.Object,
                _userRepo.Object,
                _authService.Object,
                _httpContextAccessor.Object,
                _dbContext.Object
            );
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ShouldThrowErrorException()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "notfound@example.com",
                Password = "123456"
            };

            _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TblUserAccount, bool>>>()))
                     .ReturnsAsync((TblUserAccount)null);

            // Act
            Func<Task> act = async () => await _sut.LoginAsync(request);

            // Assert
            var ex = await Assert.ThrowsAsync<ErrorException>(() => _sut.LoginAsync(request));
            Assert.Equal(StatusCodeEnum.B01, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ShouldThrowErrorException()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "admin@example.com",
                Password = "wrongpassword"
            };

            var user = new TblUserAccount
            {
                UacEmail = request.Email,
                UacPassword = new PasswordHasher<TblUserAccount>()
                                 .HashPassword(null, "correctpassword"),
                UacRole = "Admin"
            };

            _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TblUserAccount, bool>>>()))
                     .ReturnsAsync(user);

            // Act
            var ex = await Assert.ThrowsAsync<ErrorException>(() => _sut.LoginAsync(request));

            // Assert
            Assert.Equal(StatusCodeEnum.B02, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "cashier@example.com",
                Password = "cashier123"
            };

            var user = new TblUserAccount
            {
                UacEmail = request.Email,
                UacPassword = new PasswordHasher<TblUserAccount>()
                                 .HashPassword(null, request.Password),
                UacRole = "Cashier"
            };

            _userRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TblUserAccount, bool>>>()))
                     .ReturnsAsync(user);

            _authService.Setup(s => s.GenerateJwtTokenAsync(user))
                        .ReturnsAsync("fake-jwt-token");

            // Act
            var token = await _sut.LoginAsync(request);

            // Assert
            Assert.Equal("fake-jwt-token", token);
        }

    }
}
