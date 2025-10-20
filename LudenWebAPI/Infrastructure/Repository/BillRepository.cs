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
    public class BillRepository : IBillRepository
    {
        private readonly DbContext _context;

        public BillRepository(DbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Bill entity)
        {
            await _context.Set<Bill>().AddAsync(entity);
        }

        public async Task RemoveAsync(Bill entity)
        {
            _context.Set<Bill>().Remove(entity);
            await Task.CompletedTask;
        }

        public async Task RemoveByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<Bill>().Remove(entity);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Bill entity)
        {
            _context.Set<Bill>().Update(entity);
            await Task.CompletedTask;
        }

        public async Task<Bill> GetByIdAsync(int id)
        {
            return await _context.Set<Bill>().FindAsync(id);
        }

        public async Task<IEnumerable<Bill>> GetAllAsync()
        {
            return await _context.Set<Bill>().ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
