using ParkingLot.Core.DTOs;

namespace ParkingLot.Core.Interfaces;

public interface IParkingLotService
{
    Task CreateAsync(ParkingLot parkingLot);
    Task UpdateAsync(ParkingLot parkingLot);
    Task<ParkingLot> GetByIdAsync(string id);
    Task<IEnumerable<IParkingLotDto>> GetAllAsync();
    Task<IEnumerable<IParkingLotDto>> GetByOwnerIdAsync(string ownerId);
    Task<bool> IsOwnerAsync(string parkingLotId, string userId);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<IParkingLotDto>> SearchNearbyAsync(double latitude, double longitude, double radiusInKm);
}