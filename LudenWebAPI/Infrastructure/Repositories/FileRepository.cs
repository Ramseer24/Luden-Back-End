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

            if (result.RawJson.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                // Десериализуем напрямую как ImageFile
                var imageFile = JsonSerializer.Deserialize<ImageFile>(result.RawJson, JsonOptions.Default);
                return imageFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Firebase десериализация ImageFile] Ошибка: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<ImageFile>> GetFilesByProductIdAsync(ulong productId)
        {
            // Используем "files" коллекцию напрямую
            var result = await _firebaseRepo.GetAsync<Dictionary<string, ImageFile>>(CollectionName, new FirebaseConsoleListener<Dictionary<string, ImageFile>>(data => { }));
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.RawJson) || result.RawJson.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                return Enumerable.Empty<ImageFile>();

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, ImageFile>>(result.RawJson, JsonOptions.Default);
                if (dict == null)
                    return Enumerable.Empty<ImageFile>();

                return dict.Values
                    .Where(f => f.ProductId == productId)
                    .ToList();
            }
            catch
            {
                return Enumerable.Empty<ImageFile>();
            }
        }

        public async Task<ImageFile?> GetUserAvatarAsync(ulong userId)
        {
            // Используем "files" коллекцию напрямую
            var result = await _firebaseRepo.GetAsync<Dictionary<string, ImageFile>>(CollectionName, new FirebaseConsoleListener<Dictionary<string, ImageFile>>(data => { }));
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.RawJson) || result.RawJson.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                return null;

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

        public async Task<ImageFile> AddImageFileAsync(ImageFile imageFile)
        {
            // Генерируем ID если не установлен
            if (imageFile.Id == 0)
            {
                var baseId = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                // Проверяем, не существует ли уже запись с таким ID
                var existing = await GetImageFileByIdAsync(baseId);
                if (existing != null)
                {
                    // Если ID уже существует, добавляем случайное число
                    var random = new Random();
                    baseId += (ulong)random.Next(1, 10000);
                }
                imageFile.Id = baseId;
            }

            // Используем "files" коллекцию напрямую
            var result = await _firebaseRepo.SetAsync($"{CollectionName}/{imageFile.Id}", imageFile, new ConsoleFirebaseListener());
            if (!result.IsSuccess)
                throw new Exception($"Failed to add image file: {result.Message}");

            return imageFile;
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
