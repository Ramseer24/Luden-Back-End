using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BillRepository(LudenDbContext context) : GenericRepository<Bill>(context) ,IBillRepository
    {
        public async Task AddAsync(Bill entity)
        {
            await context.Set<Bill>().AddAsync(entity);
        }

        public async Task RemoveAsync(Bill entity)
        {
            context.Set<Bill>().Remove(entity);
            await Task.CompletedTask;
        }

        public async Task RemoveByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                context.Set<Bill>().Remove(entity);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Bill entity)
        {
            context.Set<Bill>().Update(entity);
            await Task.CompletedTask;
        }

        public async Task<Bill> GetByIdAsync(int id)
        {
            return await context.Set<Bill>().FindAsync(id);
        }

        public async Task<IEnumerable<Bill>> GetAllAsync()
        {
            return await context.Set<Bill>().ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
