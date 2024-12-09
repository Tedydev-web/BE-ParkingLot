using ParkingLot.Core.Entities;
using ParkingLot.Core.DTOs;
using GoongMap.SDK; // Added to resolve GeocodingResult

namespace ParkingLot.Core.Interfaces;

public interface IGoongMapService
{
    Task<GeocodingResult> GeocodeAsync(string address);
    Task<Location> GetLocationFromCoordinatesAsync(double lat, double lng);
    Task<Location> GetLocationFromAddressAsync(string address);
    Task<IEnumerable<Location>> SearchPlacesAsync(string keyword, double? lat = null, double? lng = null);
    Task<double> CalculateDistanceAsync(Location from, Location to);
    Task<Dictionary<string, double>> GetDistancesAsync(Location origin, IEnumerable<Location> destinations);
    Task<bool> ValidateAddress(string address, double latitude, double longitude);
    Task<(double latitude, double longitude)> GetCoordinates(string address);
    Task<string?> GetPlaceId(string address);
}