using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;
using ParkingLot.Core.Interfaces;
using ParkingLot.Infrastructure.Data;
using ParkingLot.Application.DTOs;

namespace ParkingLot.Application.Services;

public class ParkingLotService : IParkingLotService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IGoongMapService _goongMapService;

    public ParkingLotService(
        ApplicationDbContext context,
        IMapper mapper,
        IGoongMapService goongMapService)
    {
        _context = context;
        _mapper = mapper;
        _goongMapService = goongMapService;
    }

    public async Task<IParkingLotDto> CreateAsync(ICreateParkingLotDto dto, string userId)
    {
        var parkingLot = _mapper.Map<Core.Entities.ParkingLot>(dto);
        parkingLot.Id = Guid.NewGuid().ToString();
        parkingLot.OwnerId = userId;
        parkingLot.CreatedAt = DateTime.UtcNow;

        _context.ParkingLots.Add(parkingLot);
        await _context.SaveChangesAsync();

        return _mapper.Map<ParkingLotDto>(parkingLot);
    }

    public async Task<IParkingLotDto> GetByIdAsync(string id)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(id);
        if (parkingLot == null)
            throw new KeyNotFoundException($"Parking lot with ID {id} not found");
        
        return _mapper.Map<ParkingLotDto>(parkingLot);
    }

    public async Task<IEnumerable<ParkingLotDto>> GetAllAsync()
    {
        var parkingLots = await _parkingLotRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ParkingLotDto>>(parkingLots);
    }

    public async Task<IEnumerable<IParkingLotDto>> GetByOwnerIdAsync(string ownerId)
    {
        var parkingLots = await _context.ParkingLots
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
        return _mapper.Map<IEnumerable<ParkingLotDto>>(parkingLots);
    }

    public async Task<bool> IsOwnerAsync(string parkingLotId, string userId)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(parkingLotId);
        return parkingLot?.OwnerId == userId;
    }

    public async Task<IParkingLotDto> UpdateAsync(string id, IUpdateParkingLotDto dto)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(id);
        if (parkingLot == null)
            throw new KeyNotFoundException($"Parking lot with ID {id} not found");

        _mapper.Map(dto, parkingLot);
        parkingLot.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<ParkingLotDto>(parkingLot);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(id);
        if (parkingLot == null)
            return false;

        _context.ParkingLots.Remove(parkingLot);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<IParkingLotDto>> SearchNearbyAsync(double latitude, double longitude, double radiusInKm)
    {
        var allParkingLots = await _context.ParkingLots
            .Where(p => p.IsActive)
            .ToListAsync();

        var origin = new Location
        {
            Latitude = latitude,
            Longitude = longitude
        };

        var distances = await _goongMapService.GetDistancesAsync(origin, 
            allParkingLots.Select(p => new Location 
            { 
                Id = p.Id,
                Latitude = p.Location.Latitude,
                Longitude = p.Location.Longitude
            }));

        var nearbyParkingLots = allParkingLots
            .Where(p => distances.ContainsKey(p.Id) && distances[p.Id] <= radiusInKm * 1000)
            .ToList();

        return _mapper.Map<IEnumerable<ParkingLotDto>>(nearbyParkingLots);
    }
}