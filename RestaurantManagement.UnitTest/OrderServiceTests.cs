using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Dtos.OrdersDto;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Core.Enums;
using AutoMapper;

public class OrderServiceTests
{
    private readonly Mock<IRepository<TblOrderInfo>> _orderInfoRepoMock;
    private readonly Mock<IRepository<TblOrderDetail>> _orderDetailsRepoMock;
    private readonly Mock<IRepository<TblReservation>> _reservationRepoMock;
    private readonly Mock<IRepository<TblMenu>> _menuRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly IMapper _mapper;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderInfoRepoMock = new Mock<IRepository<TblOrderInfo>>();
        _orderDetailsRepoMock = new Mock<IRepository<TblOrderDetail>>();
        _reservationRepoMock = new Mock<IRepository<TblReservation>>();
        _menuRepoMock = new Mock<IRepository<TblMenu>>();
        _orderRepoMock = new Mock<IOrderRepository>();

        var config = new MapperConfiguration(cfg => { /* Thêm cấu hình AutoMapper nếu cần */ });
        _mapper = config.CreateMapper();

        _orderService = new OrderService(
            null, // AppSettings nếu không cần có thể để null
            _mapper,
            _orderInfoRepoMock.Object,
            _orderDetailsRepoMock.Object,
            _menuRepoMock.Object,
            _orderRepoMock.Object,
            _reservationRepoMock.Object
        );
    }

}
