using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BillRepository(LudenDbContext context) : GenericRepository<Bill>(context), IBillRepository
    {
        public ICollection<Bill> GetUserBillsByIdAsync(int id)
        {
            return context.Bills.Include(b => b.BillItems).Where(b => b.UserId == id).ToList();
        }
    }
}
