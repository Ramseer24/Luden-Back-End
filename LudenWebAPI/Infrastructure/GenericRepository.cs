using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Interfaces.Repository;
namespace Infrastructure
{
    namespace Infrastructure.Repository
    {

        public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
        {
            protected readonly DbContext _context;
            protected readonly DbSet<T> _dbSet;

            public GenericRepository(DbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
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
                await _context.SaveChangesAsync();
            }
        }
    }
}
