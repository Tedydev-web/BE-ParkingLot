using Microsoft.EntityFrameworkCore;
using ParkingLot.Core.Interfaces;
using ParkingLot.Infrastructure.Data;
using System.Linq.Expressions;

namespace ParkingLot.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await SaveChangesAsync();
        }

        public async Task<T?> GetByIdWithIncludesAsync(string id, 
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            
            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
        }

        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;
            
            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
