using ParkingLot.Core.DTOs;

namespace ParkingLot.Core.Interfaces;

public interface IParkingLotService
{
    Task<IParkingLotDto> CreateAsync(ICreateParkingLotDto dto, string userId);
    Task<IParkingLotDto> GetByIdAsync(string id);
    Task<IEnumerable<IParkingLotDto>> GetAllAsync();
    Task<IEnumerable<IParkingLotDto>> GetByOwnerIdAsync(string ownerId);
    Task<bool> IsOwnerAsync(string parkingLotId, string userId);
    Task<IParkingLotDto> UpdateAsync(string id, IUpdateParkingLotDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<IParkingLotDto>> SearchNearbyAsync(double latitude, double longitude, double radiusInKm);
} 