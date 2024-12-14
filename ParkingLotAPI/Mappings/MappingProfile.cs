using AutoMapper;
using ParkingLotAPI.DTOs;
using ParkingLotAPI.Models;

namespace ParkingLotAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ParkingLot, ParkingLotResponseDto>()
                .ForMember(dest => dest.Place_id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Formatted_address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => new Geometry
                {
                    Location = new Location
                    {
                        Lat = src.Latitude,
                        Lng = src.Longitude
                    }
                }))
                .ForMember(dest => dest.Opening_hours, opt => opt.MapFrom(src => new OpeningHours
                {
                    Open_now = src.IsOpen24Hours || IsOpenNow(src.OpeningTime, src.ClosingTime),
                    Weekday_text = new[] { $"Giờ mở cửa: {(src.IsOpen24Hours ? "24/7" : $"{src.OpeningTime} - {src.ClosingTime}")}" }
                }))
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Images.Select(img => new Photo
                {
                    Photo_reference = img.ImageUrl
                })))
                .ForMember(dest => dest.Formatted_phone_number, opt => opt.MapFrom(src => src.ContactNumber));
        }

        private bool IsOpenNow(TimeSpan? openingTime, TimeSpan? closingTime)
        {
            if (!openingTime.HasValue || !closingTime.HasValue)
                return false;
            
            var now = DateTime.Now.TimeOfDay;
            return now >= openingTime.Value && now <= closingTime.Value;
        }
    }
}
