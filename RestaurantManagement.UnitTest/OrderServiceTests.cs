using AutoMapper;
using Moq;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.DataAccess.Implementation;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos.OrdersDto;
using RestaurantManagement.Service.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

public class OrderServiceTests
{
    private readonly Mock<IRepository<TblOrderInfo>> _orderInfoRepositoryMock;
    private readonly Mock<IRepository<TblOrderDetail>> _orderDetailsRepositoryMock;
    private readonly Mock<IRepository<TblReservation>> _reservationRepositoryMock;
    private readonly Mock<IRepository<TblMenu>> _menuRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        // Khởi tạo các mock objects
        _orderInfoRepositoryMock = new Mock<IRepository<TblOrderInfo>>();
        _orderDetailsRepositoryMock = new Mock<IRepository<TblOrderDetail>>();
        _reservationRepositoryMock = new Mock<IRepository<TblReservation>>();
        _menuRepositoryMock = new Mock<IRepository<TblMenu>>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _mapperMock = new Mock<IMapper>();

        // Khởi tạo AppSettings (giả định)
        var appSettings = new AppSettings();

        // Khởi tạo OrderService với các dependency đã mock
        _orderService = new OrderService(
            appSettings,
            _mapperMock.Object,
            _orderInfoRepositoryMock.Object,
            _orderDetailsRepositoryMock.Object,
            _menuRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _reservationRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ProcessAndUpdateOrderAsync_ThrowsException_WhenNoServingReservationFound()
    {
        // Arrange
        var tbiId = Guid.NewGuid();
        var newOrderItems = new List<OrderItemDto> { new OrderItemDto { MnuID = Guid.NewGuid(), OdtQuantity = 1 } };

        _reservationRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblReservation, bool>>>()))
            .ReturnsAsync((TblReservation)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ErrorException>(() => _orderService.ProcessAndUpdateOrderAsync(tbiId, newOrderItems));
        Assert.Equal(StatusCodeEnum.D02, exception.StatusCode);
    }

    [Fact]
    public async Task ProcessAndUpdateOrderAsync_CreatesNewOrder_WhenNoExistingOrder()
    {
        // Arrange
        var tbiId = Guid.NewGuid();
        var reservation = new TblReservation { ResId = Guid.NewGuid(), TbiId = tbiId, ResStatus = "Serving", CusId = Guid.NewGuid() };
        var menuItem = new TblMenu { MnuId = new Guid("0C7F7AEE-E708-480B-B5A3-0A5A732E1549"), MnuPrice = 10.0m};
        var newOrderItems = new List<OrderItemDto> { new OrderItemDto { MnuID = menuItem.MnuId, OdtQuantity = 2 } };
        

        _reservationRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblReservation, bool>>>()))
            .ReturnsAsync(reservation);

        _orderInfoRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblOrderInfo, bool>>>()))
            .ReturnsAsync((TblOrderInfo)null);

        _menuRepositoryMock
            .Setup(repo => repo.FindByIdAsync(newOrderItems[0].MnuID))
            .ReturnsAsync(menuItem);

        _orderInfoRepositoryMock
            .Setup(repo => repo.InsertAsync(It.IsAny<TblOrderInfo>()))
            .Returns(Task.CompletedTask);

        _orderDetailsRepositoryMock
            .Setup(repo => repo.InsertAsync(It.IsAny<TblOrderDetail>()))
            .Returns(Task.CompletedTask);

        _orderInfoRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<TblOrderInfo>()))
            .Returns(Task.CompletedTask);

        var orderDto = new OrderDTO { OrdID = Guid.NewGuid(), TotalPrice = 20.0m, CusID = reservation.CusId.Value, TbiID = tbiId };
        _mapperMock
            .Setup(m => m.Map<OrderDTO>(It.IsAny<TblOrderInfo>()))
            .Returns(orderDto);

        // Act
        var result = await _orderService.ProcessAndUpdateOrderAsync(tbiId, newOrderItems);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20.0m, result.TotalPrice); // 2 * 10
        _orderInfoRepositoryMock.Verify(repo => repo.InsertAsync(It.IsAny<TblOrderInfo>()), Times.Once);
        _orderDetailsRepositoryMock.Verify(repo => repo.InsertAsync(It.IsAny<TblOrderDetail>()), Times.Once);
        _orderInfoRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<TblOrderInfo>(o => o.TotalPrice == 20.0m)), Times.Once);
    }

    [Fact]
    public async Task ProcessAndUpdateOrderAsync_UpdatesExistingOrder_WhenOrderExists()
    {
        // Arrange
        var tbiId = Guid.NewGuid();
        var reservation = new TblReservation { ResId = Guid.NewGuid(), TbiId = tbiId, ResStatus = "Serving", CusId = Guid.NewGuid() };
        var existingOrder = new TblOrderInfo { OrdId = Guid.NewGuid(), ResId = reservation.ResId, TotalPrice = 20.0m, CusId = reservation.CusId.Value, TbiId = tbiId };
        var newOrderItems = new List<OrderItemDto> { new OrderItemDto { MnuID = Guid.NewGuid(), OdtQuantity = 1 } };
        var menuItem = new TblMenu { MnuId = newOrderItems[0].MnuID, MnuPrice = 10.0m };

        _reservationRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblReservation, bool>>>()))
            .ReturnsAsync(reservation);

        _orderInfoRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblOrderInfo, bool>>>()))
            .ReturnsAsync(existingOrder);

        _orderDetailsRepositoryMock
            .Setup(repo => repo.FindListAsync(It.IsAny<Expression<Func<TblOrderDetail, bool>>>()))
            .ReturnsAsync(new List<TblOrderDetail>());

        _menuRepositoryMock
            .Setup(repo => repo.FindByIdAsync(newOrderItems[0].MnuID))
            .ReturnsAsync(menuItem);

        _orderDetailsRepositoryMock
            .Setup(repo => repo.InsertAsync(It.IsAny<TblOrderDetail>()))
            .Returns(Task.CompletedTask);

        _orderInfoRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<TblOrderInfo>()))
            .Returns(Task.CompletedTask);

        var orderDto = new OrderDTO { OrdID = existingOrder.OrdId, TotalPrice = 30.0m, CusID = reservation.CusId.Value, TbiID = tbiId };
        _mapperMock
            .Setup(m => m.Map<OrderDTO>(It.IsAny<TblOrderInfo>()))
            .Returns(orderDto);

        // Act
        var result = await _orderService.ProcessAndUpdateOrderAsync(tbiId, newOrderItems);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30.0m, result.TotalPrice); // 20 (existing) + 10 (new)
        _orderDetailsRepositoryMock.Verify(repo => repo.InsertAsync(It.IsAny<TblOrderDetail>()), Times.Once);
        _orderInfoRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<TblOrderInfo>(o => o.TotalPrice == 30.0m)), Times.Once);
    }

    [Fact]
    public async Task ProcessAndUpdateOrderAsync_ThrowsException_WhenMenuItemNotFound()
    {
        // Arrange
        var tbiId = Guid.NewGuid();
        var reservation = new TblReservation { ResId = Guid.NewGuid(), TbiId = tbiId, ResStatus = "Serving", CusId = Guid.NewGuid() };
        var newOrderItems = new List<OrderItemDto> { new OrderItemDto { MnuID = Guid.NewGuid(), OdtQuantity = 1 } };

        _reservationRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblReservation, bool>>>()))
            .ReturnsAsync(reservation);

        _orderInfoRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblOrderInfo, bool>>>()))
            .ReturnsAsync(new TblOrderInfo { OrdId = Guid.NewGuid(), ResId = reservation.ResId });

        _menuRepositoryMock
            .Setup(repo => repo.FindByIdAsync(newOrderItems[0].MnuID))
            .ReturnsAsync((TblMenu)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ErrorException>(() => _orderService.ProcessAndUpdateOrderAsync(tbiId, newOrderItems));
        Assert.Equal(StatusCodeEnum.D01, exception.StatusCode);
    }

    [Fact]
    public async Task ProcessAndUpdateOrderAsync_AccumulatesQuantity_WhenItemExists()
    {
        // Arrange
        var tbiId = Guid.NewGuid();
        var reservation = new TblReservation { ResId = Guid.NewGuid(), TbiId = tbiId, ResStatus = "Serving", CusId = Guid.NewGuid() };
        var existingOrder = new TblOrderInfo { OrdId = Guid.NewGuid(), ResId = reservation.ResId, TotalPrice = 10.0m, CusId = reservation.CusId.Value, TbiId = tbiId };
        var existingOrderDetail = new TblOrderDetail { OdtId = Guid.NewGuid(), OrdId = existingOrder.OrdId, MnuId = Guid.NewGuid(), OdtQuantity = 1 };
        var newOrderItems = new List<OrderItemDto> { new OrderItemDto { MnuID = existingOrderDetail.MnuId, OdtQuantity = 2 } };
        var menuItem = new TblMenu { MnuId = existingOrderDetail.MnuId, MnuPrice = 5.0m };

        _reservationRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblReservation, bool>>>()))
            .ReturnsAsync(reservation);

        _orderInfoRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TblOrderInfo, bool>>>()))
            .ReturnsAsync(existingOrder);

        _orderDetailsRepositoryMock
            .Setup(repo => repo.FindListAsync(It.IsAny<Expression<Func<TblOrderDetail, bool>>>()))
            .ReturnsAsync(new List<TblOrderDetail> { existingOrderDetail });

        _menuRepositoryMock
            .Setup(repo => repo.FindByIdAsync(existingOrderDetail.MnuId))
            .ReturnsAsync(menuItem);

        _orderDetailsRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<TblOrderDetail>()))
            .Returns(Task.CompletedTask);

        _orderInfoRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<TblOrderInfo>()))
            .Returns(Task.CompletedTask);

        var orderDto = new OrderDTO { OrdID = existingOrder.OrdId, TotalPrice = 20.0m, CusID = reservation.CusId.Value, TbiID = tbiId };
        _mapperMock
            .Setup(m => m.Map<OrderDTO>(It.IsAny<TblOrderInfo>()))
            .Returns(orderDto);

        // Act
        var result = await _orderService.ProcessAndUpdateOrderAsync(tbiId, newOrderItems);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20.0m, result.TotalPrice); // 10 (existing) + 2 * 5 (new)
        _orderDetailsRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<TblOrderDetail>(od => od.OdtQuantity == 3)), Times.Once);
        _orderInfoRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<TblOrderInfo>(o => o.TotalPrice == 20.0m)), Times.Once);
    }
}
