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
            CreateMap<CreateParkingLotDto, Core.Entities.ParkingLot>()
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
                .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            CreateMap<Core.Entities.ParkingLot, ParkingLotDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Location.Address))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude))
                .ForMember(dest => dest.SlotTypeCapacity, opt => opt.MapFrom(src => src.SlotTypeCapacity))
                .ForMember(dest => dest.HourlyRateByType, opt => opt.MapFrom(src => src.HourlyRateByType))
                .ForMember(dest => dest.TotalSlots, opt => opt.MapFrom(src => src.TotalSlots))
                .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.AvailableSlots))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.AverageRating))
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.TotalReviews))
                .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone))
                .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.Is24Hours, opt => opt.MapFrom(src => src.Is24Hours))
                .ForMember(dest => dest.OpenTime, opt => opt.MapFrom(src => src.OpenTime))
                .ForMember(dest => dest.CloseTime, opt => opt.MapFrom(src => src.CloseTime))
                .ForMember(dest => dest.HasCamera, opt => opt.MapFrom(src => src.HasCamera))
                .ForMember(dest => dest.HasRoof, opt => opt.MapFrom(src => src.HasRoof))
                .ForMember(dest => dest.HasOvernightParking, opt => opt.MapFrom(src => src.HasOvernightParking))
                .ForMember(dest => dest.HasDisabledAccess, opt => opt.MapFrom(src => src.HasDisabledAccess))
                .ForMember(dest => dest.HasWashing, opt => opt.MapFrom(src => src.HasWashing))
                .ForMember(dest => dest.HasMaintenance, opt => opt.MapFrom(src => src.HasMaintenance))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        }
    }
}
