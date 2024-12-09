using AutoMapper;
using ParkingLot.Application.DTOs;
using ParkingLot.Core.Entities;

namespace ParkingLot.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Core.Entities.ParkingLot, ParkingLotDto>();
        CreateMap<CreateParkingLotDto, Core.Entities.ParkingLot>();
        CreateMap<UpdateParkingLotDto, Core.Entities.ParkingLot>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ParkingSlot, ParkingSlotDto>();
        CreateMap<ParkingPrice, ParkingPriceDto>();
    }
}