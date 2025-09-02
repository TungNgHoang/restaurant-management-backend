using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.InvoiceDto;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Service.Interfaces;
using System.Linq.Expressions;


namespace RestaurantManagement.UnitTest.Services
{
    public class PaymentServiceTest
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IRepository<TblReservation>> _reservationsRepoMock;
        private readonly Mock<IRepository<TblTableInfo>> _tablesRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IRepository<TblPayment>> _paymentRepoMock;
        private readonly Mock<IRepository<TblPromotion>> _promotionRepoMock;
        private readonly Mock<IRepository<TblCustomer>> _customerRepoMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<PaymentService>> _loggerMock;
        private readonly Mock<IPayOSService> _payOSServiceMock;
        private readonly Mock<IInvoiceService> _invoiceServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<RestaurantDBContext> _dbContextMock;
        private readonly Mock<IDbContextTransaction> _transactionMock;
        private readonly PaymentService _sut;

        public PaymentServiceTest()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _reservationsRepoMock = new Mock<IRepository<TblReservation>>();
            _tablesRepoMock = new Mock<IRepository<TblTableInfo>>();
            _orderRepoMock = new Mock<IOrderRepository>();
            _paymentRepoMock = new Mock<IRepository<TblPayment>>();
            _promotionRepoMock = new Mock<IRepository<TblPromotion>>();
            _customerRepoMock = new Mock<IRepository<TblCustomer>>();
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<PaymentService>>();
            _payOSServiceMock = new Mock<IPayOSService>();
            _invoiceServiceMock = new Mock<IInvoiceService>();
            _mapperMock = new Mock<IMapper>();

            // Mock DbContext and Transaction
            var options = new DbContextOptionsBuilder<RestaurantDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContextMock = new Mock<RestaurantDBContext>(options);
            _transactionMock = new Mock<IDbContextTransaction>();

            var mockDatabase = new Mock<DatabaseFacade>(_dbContextMock.Object);
            mockDatabase.Setup(d => d.BeginTransactionAsync(default))
                .ReturnsAsync(_transactionMock.Object);

            var appSettings = new AppSettings();

            _sut = new PaymentService(
                appSettings,
                _mapperMock.Object,
                _httpContextAccessorMock.Object,
                _reservationsRepoMock.Object,
                _tablesRepoMock.Object,
                _orderRepoMock.Object,
                _paymentRepoMock.Object,
                _promotionRepoMock.Object,
                _customerRepoMock.Object,
                _notificationServiceMock.Object,
                _dbContextMock.Object,
                _loggerMock.Object,
                _payOSServiceMock.Object,
                _invoiceServiceMock.Object
            );
        }

        [Fact]
        public async Task CheckoutAndPayAsync_ShouldThrow_When_ReservationNotFound()
        {
            // Arrange
            var resId = Guid.NewGuid();
            var ordId = Guid.NewGuid();

            var order = new TblOrderInfo { OrdId = ordId, TotalPrice = 100000 };

            _orderRepoMock.Setup(o => o.GetOrderByIdAsync(ordId))
                .ReturnsAsync(order);

            _reservationsRepoMock.Setup(r => r.FindByIdAsync(resId))
                .ReturnsAsync((TblReservation)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ErrorException>(
                () => _sut.CheckoutAndPayAsync(resId, ordId, "", "Cash")
            );

            Assert.Contains("Reservation không tồn tại hoặc không ở trạng thái Serving.", exception.Message);
        }

        [Fact]
        public async Task CheckoutAndPayAsync_ShouldThrow_When_ReservationNotServing()
        {
            // Arrange
            var resId = Guid.NewGuid();
            var ordId = Guid.NewGuid();

            var order = new TblOrderInfo { OrdId = ordId, TotalPrice = 100000 };
            var reservation = new TblReservation
            {
                ResId = resId,
                ResStatus = ReservationStatus.Pending.ToString(),
                TbiId = Guid.NewGuid()
            };

            _orderRepoMock.Setup(o => o.GetOrderByIdAsync(ordId))
                .ReturnsAsync(order);

            _reservationsRepoMock.Setup(r => r.FindByIdAsync(resId))
                .ReturnsAsync(reservation);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ErrorException>(
                () => _sut.CheckoutAndPayAsync(resId, ordId, "", "Cash")
            );

            Assert.Contains("Reservation không tồn tại hoặc không ở trạng thái Serving.", exception.Message);
        }

        [Fact]
        public async Task CheckoutAndPayAsync_ShouldThrow_When_TableNotFound()
        {
            // Arrange
            var resId = Guid.NewGuid();
            var ordId = Guid.NewGuid();
            var tableId = Guid.NewGuid();

            var order = new TblOrderInfo { OrdId = ordId, TotalPrice = 100000 };
            var reservation = new TblReservation
            {
                ResId = resId,
                ResStatus = ReservationStatus.Serving.ToString(),
                TbiId = tableId
            };

            _orderRepoMock.Setup(o => o.GetOrderByIdAsync(ordId))
                .ReturnsAsync(order);

            _reservationsRepoMock.Setup(r => r.FindByIdAsync(resId))
                .ReturnsAsync(reservation);

            _tablesRepoMock.Setup(t => t.FindByIdAsync(tableId))
                .ReturnsAsync((TblTableInfo)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ErrorException>(
                () => _sut.CheckoutAndPayAsync(resId, ordId, "", "Cash")
            );

            Assert.Contains("Bàn không tồn tại hoặc không ở trạng thái Occupied.", exception.Message);
        }

        [Fact]
        public async Task CheckoutAndPayAsync_ShouldThrow_When_TableNotOccupied()
        {
            // Arrange
            var resId = Guid.NewGuid();
            var ordId = Guid.NewGuid();
            var tableId = Guid.NewGuid();

            var order = new TblOrderInfo { OrdId = ordId, TotalPrice = 100000 };
            var reservation = new TblReservation
            {
                ResId = resId,
                ResStatus = ReservationStatus.Serving.ToString(),
                TbiId = tableId
            };
            var table = new TblTableInfo
            {
                TbiId = tableId,
                TbiStatus = TableStatus.Empty.ToString(),
                TbiTableNumber = 1
            };

            _orderRepoMock.Setup(o => o.GetOrderByIdAsync(ordId))
                .ReturnsAsync(order);

            _reservationsRepoMock.Setup(r => r.FindByIdAsync(resId))
                .ReturnsAsync(reservation);

            _tablesRepoMock.Setup(t => t.FindByIdAsync(tableId))
                .ReturnsAsync(table);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ErrorException>(
                () => _sut.CheckoutAndPayAsync(resId, ordId, "", "Cash")
            );

            Assert.Contains("Bàn không tồn tại hoặc không ở trạng thái Occupied.", exception.Message);
        }


    }
}