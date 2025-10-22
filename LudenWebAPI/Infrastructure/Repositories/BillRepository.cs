using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;

namespace Infrastructure.Repositories
{
    public class BillRepository(LudenDbContext context) : GenericRepository<Bill>(context), IBillRepository
    {

    }
}
