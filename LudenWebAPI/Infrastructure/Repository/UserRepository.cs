using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _context;

        public UserRepository(DbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User entity)
        {
            await _context.Set<User>().AddAsync(entity);
        }

        public async Task RemoveAsync(User entity)
        {
            _context.Set<User>().Remove(entity);
            await Task.CompletedTask;
        }

        public async Task RemoveByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<User>().Remove(entity);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(User entity)
        {
            _context.Set<User>().Update(entity);
            await Task.CompletedTask;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Set<User>().FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Set<User>().ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
