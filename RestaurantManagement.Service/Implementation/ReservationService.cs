using AutoMapper;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Dtos;
using RestaurantManagement.Service.Dtos.ReserDto;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantManagement.DataAccess.Implementation;
using RestaurantManagement.Core.Exceptions;
using RestaurantManagement.Service.ApiModels;

namespace RestaurantManagement.Service.Implementation
{
    public class ReservationService : BaseService, IReservationService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblTableInfo> _tablesRepository;
        private readonly IReservationRepository _reservationRepository;
        
        public ReservationService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblReservation> reservationsRepository,
            IRepository<TblTableInfo> tablesRepository,
            IReservationRepository reservationRepository
            ) : base(appSettings, mapper)
        {
            _reservationsRepository = reservationsRepository;
            _tablesRepository = tablesRepository;
            _reservationRepository = reservationRepository;
            _mapper = mapper;
        }

        public async Task<List<AvailableTableDto>> GetAvailableTablesAsync(CheckAvailabilityRequestDto request)
        {
            var startTime = request.ResDate;
            var endTime = request.ResEndDate;
            
            var allTables = await _tablesRepository.ActiveRecordsAsync();
            var overlappingReservations = await _reservationRepository.GetOverlappingReservationsAsync(startTime, endTime);
            var occupiedTableIds = overlappingReservations.Select(r => r.TbiId).Distinct().ToList();

            var availableTables = allTables
                .Where(t => !occupiedTableIds.Contains(t.TbiId))
                .ToList();

            return _mapper.Map<List<AvailableTableDto>>(availableTables);
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationRequestDto request)
        {
            var startTime = request.ResDate;
            var endTime = request.ResEndTime;

            if (startTime > endTime)
            {
                throw new ErrorException(StatusCodeEnum.C02);
            }
            if (request.TempCustomerPhone.Length != 10)
            {
                throw new ErrorException(StatusCodeEnum.C03);
            }
           
            var overlappingReservations = await _reservationRepository.GetOverlappingReservationsAsync(startTime, endTime);
            if (overlappingReservations.Any(r => r.TbiId == request.TbiId))
            {
                throw new ErrorException(StatusCodeEnum.C01);
            }

            var reservation = new TblReservation
            {
                ResId = Guid.NewGuid(),
                TbiId = request.TbiId,
                TempCustomerName = request.TempCustomerName,
                TempCustomerPhone = request.TempCustomerPhone,
                ResDate = request.ResDate,
                ResEndTime = request.ResEndTime,
                ResNumber = request.ResNumber,
                ResStatus = ReservationStatus.Pending.ToString(),
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty // Giả định tạm thời, có thể thay đổi
            };

            await _reservationsRepository.InsertAsync(reservation);
            return _mapper.Map<ReservationResponseDto>(reservation);
        }

        public async Task<IEnumerable<ReserDto>> GetAllReservationsAsync(ReserModel pagingModel)
        {
            // Validate PageIndex and PageSize
            ValidatePagingModel(pagingModel);

            var reservations = await _reservationsRepository.AsNoTrackingAsync();
            var tables = await _tablesRepository.AsNoTrackingAsync();

            // Join Reservation và Table
            var data = from reservation in reservations
                       join table in tables on reservation.TbiId equals table.TbiId
                       select new
                       {
                           reservation.TempCustomerName,
                           reservation.TempCustomerPhone,
                           reservation.ResDate,
                           reservation.ResEndTime,
                           reservation.ResStatus,
                           reservation.ResNumber,
                           table.TbiTableNumber
                       };

            // Ánh xạ sang ReserDto với tách ngày và giờ
            var reserDto = data.Select(x => new ReserDto
            {
                TableNumber = x.TbiTableNumber,
                CustomerName = x.TempCustomerName,
                ContactPhone = x.TempCustomerPhone,
                ReservationDate = x.ResDate.Date,
                TimeIn = x.ResDate.TimeOfDay,
                TimeOut = x.ResEndTime?.TimeOfDay ?? TimeSpan.Zero,
                NumberOfPeople = x.ResNumber,
                Status = x.ResStatus
            }).ToList();
            // Apply search filter on the DTOs
            var result = AdvancedFilter(reserDto.AsEnumerable(), pagingModel, nameof(ReserDto.ReservationDate));

            return result;
        }

        private void ValidatePagingModel(ReserModel pagingModel)
        {
            if (pagingModel.PageIndex < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageIndexInvalid);
            if (pagingModel.PageSize < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageSizeInvalid);
        }
    }
}
