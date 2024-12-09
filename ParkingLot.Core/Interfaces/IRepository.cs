using System.Linq.Expressions;

namespace ParkingLot.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdWithIncludesAsync(string id, 
            Func<IQueryable<T>, IQueryable<T>>? include = null);
            
        Task<IEnumerable<T>> GetAllWithIncludesAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null);
            
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveChangesAsync();
    }
}
