using AutoMapper;
using ParkingLot.Core.Entities;
using ParkingLot.Application.DTOs;

namespace ParkingLot.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Core.Entities.ParkingLot, ParkingLotDto>()
                .ForMember(dest => dest.SlotTypes, opt => opt
                    .MapFrom(src => src.SlotTypes ?? new Dictionary<string, int>()))
                .ForMember(dest => dest.PricesByVehicleType, opt => opt
                    .MapFrom(src => src.PricesByVehicleType ?? new Dictionary<string, decimal>()))
                .ForMember(dest => dest.Images, opt => opt
                    .MapFrom(src => src.Images ?? new List<string>()));

            CreateMap<CreateParkingLotDto, Core.Entities.ParkingLot>();
            CreateMap<UpdateParkingLotDto, Core.Entities.ParkingLot>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
