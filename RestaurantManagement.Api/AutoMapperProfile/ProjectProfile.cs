using AutoMapper;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.Dtos;
using RestaurantManagement.Service.Dtos.ReserDto;

namespace RestaurantManagement.Api.AutoMapperProfile
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile() 
        {
            CreateMap<TblReservation, ReservationResponseDto>();
            CreateMap<TblTableInfo, AvailableTableDto>();
        }
    }
}
