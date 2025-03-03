using AutoMapper;
using RestaurantManagement.Api.Models;
using RestaurantManagement.Service.Dtos;

namespace RestaurantManagement.Api.AutoMapperProfile
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile() 
        {
            CreateMap<TblReservation, ReservationDto>();
        }
    }
}
