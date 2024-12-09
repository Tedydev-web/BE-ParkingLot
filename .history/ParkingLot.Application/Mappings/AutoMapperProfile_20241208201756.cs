using AutoMapper;
using ParkingLot.Core.Entities;
using ParkingLot.Application.DTOs;

namespace ParkingLot.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Core.Entities.ParkingLot, ParkingLotDto>();
            CreateMap<CreateParkingLotDto, Core.Entities.ParkingLot>();
            CreateMap<UpdateParkingLotDto, Core.Entities.ParkingLot>();
        }
    }
}
