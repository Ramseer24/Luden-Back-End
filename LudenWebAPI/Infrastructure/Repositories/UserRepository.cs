using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;

namespace Infrastructure.Repositories
{
    public class UserRepository(LudenDbContext context) : GenericRepository<User>(context), IUserRepository
    {
    }
}
