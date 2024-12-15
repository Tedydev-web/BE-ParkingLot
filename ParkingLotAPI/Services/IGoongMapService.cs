using System.Threading.Tasks;
using ParkingLotAPI.DTOs;

namespace ParkingLotAPI.Services
{
    public interface IGoongMapService
    {
        Task<CreateParkingLotDto> GetPlaceDetails(double lat, double lng);
    }
} 