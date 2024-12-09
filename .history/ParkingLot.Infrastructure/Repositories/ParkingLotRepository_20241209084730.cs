public async Task<IEnumerable<Core.Entities.ParkingLot>> GetAllAsync()
{
    return await _context.ParkingLots
        .AsNoTracking()
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
}
