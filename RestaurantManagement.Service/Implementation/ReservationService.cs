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
    public class ReservationService : BaseService, IReservationService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblTableInfo> _tablesRepository;
        private readonly IReservationRepository _reservationRepository;
        public ReservationService(
            AppSettings appSettings,
            IMapper mapper
            ) : base(appSettings, mapper)
        {
        }

        public async Task<ReservationDto> CreateReservationAsync(ReservationDto reservationDto)
        {
            // Validate input
            if (reservationDto.ResNumber <= 0 || reservationDto.ResDate < DateTime.Now)
                throw new ArgumentException("Thông tin đặt bàn không hợp lệ");

            // Giả sử thời gian dùng bữa mặc định là 2 tiếng
            var estimatedDuration = TimeSpan.FromHours(2);
            var startTime = reservationDto.ResTime.ToTimeSpan();
            var endTime = startTime + estimatedDuration;

            // Lấy danh sách bàn trống
            var availableTables = await GetAvailableTablesAsync(
                reservationDto.ResDate,
                reservationDto.ResTime,
                TimeOnly.FromTimeSpan(endTime),
                reservationDto.ResNumber
            );

            if (!availableTables.Any())
                throw new Exception("Không còn bàn trống trong khung giờ yêu cầu");

            // Chọn bàn đầu tiên trong danh sách bàn trống
            var selectedTable = availableTables.First();

            // Map từ DTO sang entity
            var reservationEntity = _mapper.Map<TblReservation>(reservationDto);
            reservationEntity.TbiId = selectedTable.TbiId;
            reservationEntity.ResStatus = "Pending";
            reservationEntity.CreatedAt = DateTime.Now;
            reservationEntity.IsDeleted = false;
            reservationEntity.CreatedBy = reservationDto.CreatedBy ?? Guid.Empty;

            // Thêm vào database
            await _reservationsRepository.InsertAsync(reservationEntity);

            // Map lại sang DTO để trả về
            var resultDto = _mapper.Map<ReservationDto>(reservationEntity);
            resultDto.TableNumber = selectedTable.TbiTableNumber;

            return resultDto;
        }

        private async Task<List<TblTableInfo>> GetAvailableTablesAsync(DateTime date, TimeOnly startTime, TimeOnly endTime, int numberOfPeople)
        {
            // Lấy danh sách các bàn hiện có
            var allTables = await _tablesRepository.ActiveRecordsAsync();

            // Lấy các reservation trong cùng khung giờ
            var existingReservations = await _reservationRepository.GetReservationsByTimeRange(date, startTime, endTime);

            // Tìm các bàn còn trống
            var occupiedTableIds = existingReservations.Select(r => r.TbiId).ToList();
            return allTables
                .Where(t => !occupiedTableIds.Contains(t.TbiId))
                .ToList();
        }
    }
}
