using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;
using System.Text.Json;
using Infrastructure.Repositories;

namespace Infrastructure.Repositories
{
    public class FileRepository : GenericRepository<ImageFile>, IFileRepository
    {
        private readonly FirebaseRepository _firebaseRepo;
        private const string CollectionName = "files"; // Используем "files" для обратной совместимости

        public FileRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
            _firebaseRepo = firebaseRepo;
        }

        public async Task<ImageFile?> GetImageFileByIdAsync(ulong id)
        {
            // Получаем сырой JSON из Firebase и десериализуем как ImageFile
            // Используем "files" как имя коллекции (для обратной совместимости с существующими данными)
            var result = await _firebaseRepo.GetAsync<ImageFile>($"files/{id}", new FirebaseConsoleListener<ImageFile>(data => { }));
            
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.RawJson))
                return null;

        public async Task<PhotoFile?> GetPhotoFileByIdAsync(ulong id)
        {
            if (_useFirebase)
            {
                var result = await GetByIdAsync(id);
                return result as PhotoFile;
            }

            return await _context!.Set<PhotoFile>()
                .Include(pf => pf.User)
                .FirstOrDefaultAsync(pf => pf.Id == id);
        }

        public async Task<ProductFile?> GetProductFileByIdAsync(ulong id)
        {
            // Используем "files" коллекцию напрямую
            var result = await _firebaseRepo.GetAsync<Dictionary<string, ImageFile>>(CollectionName, new FirebaseConsoleListener<Dictionary<string, ImageFile>>(data => { }));
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.RawJson) || result.RawJson.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                return Enumerable.Empty<ImageFile>();

            try
            {
                var result = await GetByIdAsync(id);
                return result as ProductFile;
            }

            return await _context!.Set<ProductFile>()
                .Include(pf => pf.Product)
                .FirstOrDefaultAsync(pf => pf.Id == id);
        }

        public async Task<IEnumerable<ProductFile>> GetFilesByProductIdAsync(ulong productId)
        {
            if (_useFirebase)
            {
                var all = await GetAllAsync();
                return all
                    .OfType<ProductFile>()
                    .Where(pf => pf.ProductId == productId)
                    .OrderBy(pf => pf.DisplayOrder)
                    .ToList();
            }

            return await _context!.Set<ProductFile>()
                .Where(pf => pf.ProductId == productId)
                .OrderBy(pf => pf.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PhotoFile?> GetUserAvatarAsync(ulong userId)
        {
            if (_useFirebase)
            {
                var all = await GetAllAsync();
                return all.OfType<PhotoFile>().FirstOrDefault(pf => pf.UserId == userId);
            }

            return await _context!.Set<PhotoFile>()
                .FirstOrDefaultAsync(pf => pf.UserId == userId);
        }

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, ImageFile>>(result.RawJson, JsonOptions.Default);
                if (dict == null)
                    return null;

                return dict.Values.FirstOrDefault(f => f.UserId == userId);
            }
            catch
            {
                return null;
            }
        }

        public async Task DeletePhotoFileAsync(ulong id)
        {
            // Генерируем ID если не установлен
            if (imageFile.Id == 0)
            {
                await RemoveByIdAsync(id);
                return;
            }

            var photoFile = await GetPhotoFileByIdAsync(id);
            if (photoFile != null)
            {
                _context!.Set<PhotoFile>().Remove(photoFile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductFileAsync(ulong id)
        {
            if (_useFirebase)
            {
                await RemoveByIdAsync(id);
                return;
            }

        public async Task DeleteImageFileAsync(ulong id)
        {
            // Используем "files" коллекцию напрямую
            var result = await _firebaseRepo.DeleteAsync($"{CollectionName}/{id}", new ConsoleFirebaseListener());
            if (!result.IsSuccess)
                throw new Exception($"Failed to delete image file: {result.Message}");
        }
    }
}
