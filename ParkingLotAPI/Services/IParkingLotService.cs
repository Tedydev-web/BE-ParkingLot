using System.Threading.Tasks;
using System.Collections.Generic;
using ParkingLotAPI.DTOs;

namespace ParkingLotAPI.Services
{
    public interface IParkingLotService
    {
        Task<SearchResultDto> SearchParkingLots(double latitude, double longitude, int radius);
        Task<ParkingLotResponseDto> GetParkingLotById(string id);
        Task<ParkingLotResponseDto> CreateParkingLot(CreateParkingLotDto createDto);
        Task<SearchResultDto> GetAllParkingLots();
        Task<ParkingLotResponseDto> UpdateParkingLot(string id, CreateParkingLotDto updateDto);
        Task DeleteParkingLot(string id);
        Task<SearchResultDto> SearchNearbyParkingLots(double latitude, double longitude, int radius, int limit);
    }
} 