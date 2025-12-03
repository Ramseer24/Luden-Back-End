using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
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
            var allUsers = await GetAllAsync();
            return allUsers.FirstOrDefault(u => u.Email == email);
        }

        // Получить пользователя по Google ID
        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            var allUsers = await GetAllAsync();
            return allUsers.FirstOrDefault(u => u.GoogleId == googleId);
        }

        // Проверка пароля по email
        public async Task<bool> IsPasswordValidByEmailAsync(string email, string passwordHash)
        {
            var allUsers = await GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Email == email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return false;
            return user.PasswordHash == passwordHash;
        }

        // Получить все счета пользователя по его ID
        public async Task<ICollection<Bill>> GetUserBillsByIdAsync(ulong id)
        {
            // Firebase-режим — получаем все счета и фильтруем
            var billsRepo = new BillRepository(new FirebaseRepository(new FirebaseService()));
            var allBills = await billsRepo.GetAllAsync();
            return allBills.Where(b => b.UserId == id).ToList();
        }

        // Получить все продукты, оплаченные пользователем
        public async Task<ICollection<Product>> GetUserProductsByIdAsync(ulong userId)
        {
            // Firebase-режим: достаём все счета, берём оплаченные продукты
            var billsRepo = new BillRepository(new FirebaseRepository(new FirebaseService()));
            var allBills = await billsRepo.GetAllAsync();
            var paidBills = allBills.Where(b =>
                b.UserId == userId &&
                (b.Status == Entities.Enums.BillStatus.Paid ||
                 b.Status == Entities.Enums.BillStatus.Completed)).ToList();

            // Если нет оплаченных счетов, возвращаем пустой список
            if (!paidBills.Any())
            {
                return new List<Product>();
            }

            // Загружаем все BillItems
            var billItemsRepo = new GenericRepository<BillItem>(new FirebaseRepository(new FirebaseService()));
            var allBillItems = await billItemsRepo.GetAllAsync();

            // Получаем ID оплаченных счетов
            var paidBillIds = paidBills.Select(b => b.Id).ToHashSet();

            // Фильтруем BillItems только для оплаченных счетов
            var productIds = allBillItems
                .Where(bi => paidBillIds.Contains(bi.BillId))
                .Select(bi => bi.ProductId)
                .Distinct()
                .ToList();

            // Если нет продуктов, возвращаем пустой список
            if (!productIds.Any())
            {
                return new List<Product>();
            }

            // Получаем все продукты из Firebase
            var productsRepo = new GenericRepository<Product>(new FirebaseRepository(new FirebaseService()));
            var allProducts = await productsRepo.GetAllAsync();

            var userProducts = allProducts
                .Where(p => productIds.Contains(p.Id))
                .ToList();

            // Загружаем связанные данные для каждого продукта
            var filesRepo = new GenericRepository<ImageFile>(new FirebaseRepository(new FirebaseService()));
            var regionsRepo = new GenericRepository<Region>(new FirebaseRepository(new FirebaseService()));
            var licensesRepo = new GenericRepository<License>(new FirebaseRepository(new FirebaseService()));

            var allFiles = await filesRepo.GetAllAsync();
            var allRegions = await regionsRepo.GetAllAsync();
            var allLicenses = await licensesRepo.GetAllAsync();

            // Заполняем связанные данные
            foreach (var product in userProducts)
            {
                // Загружаем файлы продукта
                product.Files = allFiles.Where(f => f.ProductId == product.Id).ToList();

                // Загружаем регион
                if (product.RegionId > 0)
                {
                    product.Region = allRegions.FirstOrDefault(r => r.Id == product.RegionId);
                }

                // Загружаем лицензии продукта
                product.Licenses = allLicenses.Where(l => l.ProductId == product.Id).ToList();

                // Устанавливаем CoverFile если есть CoverFileId
                if (product.CoverFileId.HasValue)
                {
                    product.CoverFile = allFiles.FirstOrDefault(f => f.Id == product.CoverFileId.Value);
                }
            }

            return userProducts;
        }
    }
}
