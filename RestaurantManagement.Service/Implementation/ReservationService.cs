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
        public ReservationService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblReservation> reservationRepository
            ) : base(appSettings, mapper)
        {
            _mapper = mapper;
            _reservationsRepository = reservationRepository;
        }

        public async Task<ReservationDto> CreateReservationAsync(ReservationDto reservationDto)
        {
            // Map từ DTO sang entity
            var reservationEntity = _mapper.Map<TblReservation>(reservationDto);

            // Có thể thiết lập thêm các trường mặc định (CreatedBy, …) nếu cần
            await _reservationsRepository.InsertAsync(reservationEntity);

            // Map lại từ entity sang DTO để trả về
            return _mapper.Map<ReservationDto>(reservationEntity);
        }

        public async Task<ReservationDto> GetReservationByIdAsync(Guid id)
        {
            var reservationEntity = await _reservationsRepository.FindByIdAsync(id);
            if (reservationEntity == null)
                return null;

            return _mapper.Map<ReservationDto>(reservationEntity);
        }
    }
}
