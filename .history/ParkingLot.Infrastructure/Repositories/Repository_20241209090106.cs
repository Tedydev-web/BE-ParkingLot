public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> GetByIdWithIncludesAsync(string id, 
        Func<IQueryable<T>, IQueryable<T>> include = null)
    {
        IQueryable<T> query = _dbSet;
        
        if (include != null)
        {
            query = include(query);
        }

        // Assuming entity has Id property
        return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
    }

    public async Task<IEnumerable<T>> GetAllWithIncludesAsync(
        Func<IQueryable<T>, IQueryable<T>> include = null)
    {
        IQueryable<T> query = _dbSet;
        
        if (include != null)
        {
            query = include(query);
        }

        return await query.ToListAsync();
    }
}
