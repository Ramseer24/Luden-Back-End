using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository(LudenDbContext context) : GenericRepository<User>(context), IUserRepository
    {
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<bool> IsPasswordValidByEmailAsync(string email, string passwordHash)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            return user.PasswordHash == passwordHash;
        }
        
        public async Task<User> GetUserWithBillsAsync(int id)
        {
            return await context.Users
                .Include(u => u.Bills)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
