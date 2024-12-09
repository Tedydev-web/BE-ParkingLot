using AutoMapper;
using ParkingLot.Application.DTOs;
using ParkingLot.Core.Entities;
using System.Text.Json;
using System.Linq;

namespace ParkingLot.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateParkingLotDto, ParkingLot>()
                .ForMember(dest => dest.SlotTypeCapacity, opt => opt.MapFrom(src => src.SlotTypes))
                .ForMember(dest => dest.HourlyRateByType, opt => opt.MapFrom(src => src.PricesByVehicleType))
                .ForMember(dest => dest.TotalSlots, opt => opt.MapFrom(src => src.SlotTypes.Values.Sum()))
                .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.SlotTypes.Values.Sum()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new Location
                {
                    Address = src.Address,
                    Latitude = src.Latitude,
                    Longitude = src.Longitude
                }))
                .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone))
                .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail));

            CreateMap<ParkingLot, ParkingLotDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Location.Address))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude))
                .ForMember(dest => dest.SlotTypeCapacity, opt => opt.MapFrom(src => src.SlotTypeCapacity))
                .ForMember(dest => dest.HourlyRateByType, opt => opt.MapFrom(src => src.HourlyRateByType))
                .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone))
                .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail));
        }
    }
}
