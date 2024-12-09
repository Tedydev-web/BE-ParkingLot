public class ParkingLotService : IParkingLotService
{
    private readonly ApplicationDbContext _context;

    public ParkingLotService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(ParkingLot parkingLot)
    {
        _context.ParkingLots.Add(parkingLot);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ParkingLot parkingLot)
    {
        _context.ParkingLots.Update(parkingLot);
        await _context.SaveChangesAsync();
    }

    public async Task<ParkingLot> GetByIdAsync(string id)
    {
        return await _context.ParkingLots
            .Include(p => p.Location)
            // ...include các navigation properties nếu cần...
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // ...existing code...
}
