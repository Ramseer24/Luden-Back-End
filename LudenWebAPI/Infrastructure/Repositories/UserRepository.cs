using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Extentions;
using Infrastructure.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        public async Task<ICollection<Bill>> GetUserBillsByIdAsync(int id)
        {
            return await context.Bills
                .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
                .Where(b => b.UserId == id)
                .ToListAsync();
        }

        public async Task<ICollection<Product>> GetUserProductsByIdAsync(int userId)
        {
            // Получаем уникальные ID продуктов из оплаченных счетов
            var productIds = await context.Bills
                .Where(b => b.UserId == userId &&
                           (b.Status == Entities.Enums.BillStatus.Paid ||
                            b.Status == Entities.Enums.BillStatus.Completed))
                .SelectMany(b => b.BillItems)
                .Select(bi => bi.ProductId)
                .Distinct()
                .ToListAsync();

            // Загружаем продукты с Files и Region
            var products = await context.Products
                .Where(p => productIds.Contains(p.Id.ToInt()))
                .Include(p => p.Files)
                .Include(p => p.Region)
                .ToListAsync();

            return products;
        }
    }
}
