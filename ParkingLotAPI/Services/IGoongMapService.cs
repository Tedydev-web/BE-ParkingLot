using System.Threading.Tasks;
using ParkingLotAPI.DTOs;

namespace ParkingLotAPI.Services
{
    public interface IGoongMapService
    {
        Task<GoongMapDto> GetLocationInfo(double latitude, double longitude);
        Task<ParkingLotResponseDto> CreateParkingLotFromLocation(double latitude, double longitude, CreateParkingLotDto additionalInfo);
    }
} 