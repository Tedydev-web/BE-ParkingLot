using System.Threading.Tasks;
using ParkingLotAPI.DTOs;

namespace ParkingLotAPI.Services
{
    public interface IGoongMapService
    {
        Task<CreateParkingLotDto> GetPlaceDetails(double lat, double lng);
        Task<GeocodingResultDto> GetGeocodingInfo(double lat, double lng);
        Task<PlaceDetailDto> GetPlaceDetail(string placeId);
        Task<AutocompleteResultDto> GetPlaceSuggestions(string keyword);
    }
} 