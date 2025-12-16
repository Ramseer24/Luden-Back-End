using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;
using System.Text.Json;
using Infrastructure.Repositories;

namespace Infrastructure.Repositories
{
    public class FileRepository : GenericRepository<ImageFile>, IFileRepository
    {
        public FileRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo, "files")
        {
        }

        public async Task<IEnumerable<ImageFile>> GetFilesByProductIdAsync(ulong productId)
        {
            var all = await GetAllAsync();
            return all.Where(f => f.ProductId == productId).ToList();
        }

        public async Task<ImageFile?> GetUserAvatarAsync(ulong userId)
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(f => f.UserId == userId);
        }

        // Реализация методов интерфейса IFileRepository, которые отсутствуют в IGenericRepository
        // Эти методы необходимы для совместимости с интерфейсом IFileRepository
        // Если они дублируют функциональность IGenericRepository, можно вызвать соответствующие методы базового класса
    }
}
