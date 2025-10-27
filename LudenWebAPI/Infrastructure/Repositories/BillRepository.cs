using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BillRepository(LudenDbContext context) : GenericRepository<Bill>(context), IBillRepository
    {
        public async Task<IEnumerable<Bill>> GetBillsByUserIdAsync(int userId)
        {
            return await context.Bills
                .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}
