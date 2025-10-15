using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure
{
    namespace Infrastructure.Repository
    {

        public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
        {
            protected readonly LudenDbContext context;
            protected readonly DbSet<T> _dbSet;

            public GenericRepository(LudenDbContext context)
            {
                this.context = context ?? throw new ArgumentNullException(nameof(context));
                _dbSet = context.Set<T>();
            }

            public async Task AddAsync(T entity)
            {
                await _dbSet.AddAsync(entity);
            }

            public async Task RemoveAsync(T entity)
            {
                _dbSet.Remove(entity);
            }

            public async Task RemoveByIdAsync(int id)
            {
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                }
            }

            public async Task UpdateAsync(T entity)
            {
                _dbSet.Update(entity);
            }

            public async Task<T> GetByIdAsync(int id)
            {
                return await _dbSet.FindAsync(id);
            }

            public async Task<IEnumerable<T>> GetAllAsync()
            {
                return await _dbSet.AsNoTracking().ToListAsync();
            }

            public async Task SaveChangesAsync()
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
