using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;
using Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly LudenDbContext? _context;
        private readonly bool _useFirebase;

        // Конструктор для старого режима (SQLite / EF Core)
        public UserRepository(LudenDbContext context) : base(null!)
        {
            _context = context;
            _useFirebase = false;
        }

        // Конструктор для нового режима (Firebase)
        public UserRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
            _useFirebase = true;
        }

        // Проверка, существует ли пользователь по email
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            // Пробуем получить всех пользователей из Firebase
            var allUsers = await GetAllAsync();

            if (allUsers == null || !allUsers.Any())
                return false;

            // Проверяем, есть ли совпадение по Email
            return allUsers.Any(u => u != null && u.Email == email);
        }


        // Получить пользователя по email
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (_useFirebase)
            {
                var allUsers = await GetAllAsync();
                return allUsers.FirstOrDefault(u => u.Email == email);
            }

            return await _context!.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Получить пользователя по Google ID
        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            if (_useFirebase)
            {
                var allUsers = await GetAllAsync();
                return allUsers.FirstOrDefault(u => u.GoogleId == googleId);
            }

            return await _context!.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        // Проверка пароля по email
        public async Task<bool> IsPasswordValidByEmailAsync(string email, string passwordHash)
        {
            if (_useFirebase)
            {
                var allUsers = await GetAllAsync();
                var user = allUsers.FirstOrDefault(u => u.Email == email);
                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                    return false;
                return user.PasswordHash == passwordHash;
            }

            var userDb = await _context!.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (userDb == null || string.IsNullOrEmpty(userDb.PasswordHash))
                return false;
            return userDb.PasswordHash == passwordHash;
        }

        // Получить все счета пользователя по его ID
        public async Task<ICollection<Bill>> GetUserBillsByIdAsync(ulong id)
        {
            if (_useFirebase)
            {
                // Firebase-режим — получаем все счета и фильтруем
                var billsRepo = new BillRepository(new FirebaseRepository(new FirebaseService()));
                var allBills = await billsRepo.GetAllAsync();
                return allBills.Where(b => b.UserId == id).ToList();
            }

            return await _context!.Bills
                .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
                .Where(b => b.UserId == id)
                .ToListAsync();
        }

        // Получить все продукты, оплаченные пользователем
        public async Task<ICollection<Product>> GetUserProductsByIdAsync(ulong userId)
        {
            if (_useFirebase)
            {
                // Firebase-режим: достаём все счета, берём оплаченные продукты
                var billsRepo = new BillRepository(new FirebaseRepository(new FirebaseService()));
                var allBills = await billsRepo.GetAllAsync();
                var paidBills = allBills.Where(b =>
                    b.UserId == userId &&
                    (b.Status == Entities.Enums.BillStatus.Paid ||
                     b.Status == Entities.Enums.BillStatus.Completed)).ToList();

                var productIds = paidBills
                    .SelectMany(b => b.BillItems.Select(bi => bi.ProductId))
                    .Distinct()
                    .ToList();

                // Получаем все продукты из Firebase
                var productsRepo = new GenericRepository<Product>(new FirebaseRepository(new FirebaseService()));
                var allProducts = await productsRepo.GetAllAsync();

                return allProducts
                    .Where(p => productIds.Contains(p.Id))
                    .ToList();
            }

            // Старый EF Core-режим
            var productIdsEf = await _context!.Bills
                .Where(b => b.UserId == userId &&
                            (b.Status == Entities.Enums.BillStatus.Paid ||
                             b.Status == Entities.Enums.BillStatus.Completed))
                .SelectMany(b => b.BillItems)
                .Select(bi => bi.ProductId)
                .Distinct()
                .ToListAsync();

            return await _context.Products
                .Where(p => productIdsEf.Contains(p.Id))
                .Include(p => p.Files)
                .Include(p => p.Region)
                .ToListAsync();
        }
    }
}
