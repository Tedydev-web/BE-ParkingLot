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
                .ForMember(dest => dest.Opening_hours, opt => opt.MapFrom(src => BuildOpeningHours(src)))
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Images != null 
                    ? src.Images.Select(img => new Photo
                    {
                        Photo_reference = img.ImageUrl,
                        IsMain = img.IsMain,
                        CreatedAt = img.CreatedAt
                    })
                    : new List<Photo>()))
                .ForMember(dest => dest.Formatted_phone_number, opt => opt.MapFrom(src => src.ContactNumber));
        }

        private static OpeningHours BuildOpeningHours(ParkingLot parkingLot)
        {
            string formatTime(TimeSpan? time) => 
                parkingLot.IsOpen24Hours ? "24/7" : 
                time.HasValue ? time.Value.ToString(@"HH\:mm") : "Chưa cập nhật";

            return new OpeningHours
            {
                Open_now = IsOpenNow(parkingLot.OpeningTime, parkingLot.ClosingTime, parkingLot.IsOpen24Hours),
                Operating_hours = new OperatingTime
                {
                    Open = formatTime(parkingLot.OpeningTime),
                    Close = formatTime(parkingLot.ClosingTime),
                    Is24Hours = parkingLot.IsOpen24Hours
                }
            };
        }

        private static bool IsOpenNow(TimeSpan? openingTime, TimeSpan? closingTime, bool isOpen24Hours)
        {
            if (isOpen24Hours)
                return true;

            if (!openingTime.HasValue || !closingTime.HasValue)
                return false;
            
            var now = DateTime.Now.TimeOfDay;
            
            // Xử lý trường hợp qua ngày
            if (closingTime.Value < openingTime.Value)
            {
                return now >= openingTime.Value || now <= closingTime.Value;
            }

            return now >= openingTime.Value && now <= closingTime.Value;
        }
    }
}
