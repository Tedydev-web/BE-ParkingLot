using AutoMapper;
using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;
using System.Text.Json;
using System.Linq;

namespace ParkingLot.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map từ CreateParkingLotDto sang ParkingLot
            CreateMap<CreateParkingLotDto, ParkingLot>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new Location
                {
                    Address = src.Address,
                    Latitude = src.Latitude,
                    Longitude = src.Longitude
                }))
                .ForMember(dest => dest.HourlyRateByType, opt => opt.MapFrom(src => src.PricesByVehicleType))
                .ForMember(dest => dest.SlotTypeCapacity, opt => opt.MapFrom(src => src.SlotTypeCapacity))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
                // ...mapping các thuộc tính khác...

            // Map từ UpdateParkingLotDto sang ParkingLot
            CreateMap<UpdateParkingLotDto, ParkingLot>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
                // ...mapping các thuộc tính khác...

            // Map từ ParkingLot sang ParkingLotDto
            CreateMap<ParkingLot, ParkingLotDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Location.Address))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude));
                // ...mapping các thuộc tính khác...
        }
    }
}
