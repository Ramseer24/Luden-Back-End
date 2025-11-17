using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Models;
using SixLabors.ImageSharp;

namespace Application.Services
{
    public class FileService : GenericService<ImageFile>, IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly string _baseUrl;

        public FileService(
            IFileRepository fileRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService) : base(fileRepository)
        {
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
            _baseUrl = "/uploads"; // Базовый URL для доступа к файлам
        }

        public async Task<ImageFile> UploadImageAsync(ulong? userId, ulong? productId, Stream fileStream, string fileName, string contentType, long fileSize)
        {
            // Проверяем, что передан либо userId, либо productId
            if (!userId.HasValue && !productId.HasValue)
            {
                throw new InvalidOperationException("Either userId or productId must be provided");
            }

            // Если это аватар пользователя, проверяем существование пользователя и удаляем старый аватар
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {userId.Value} not found");
                }

                // Удаляем старый аватар если есть
                var existingAvatar = await _fileRepository.GetUserAvatarAsync(userId.Value);
                if (existingAvatar != null)
                {
                    await _fileStorageService.DeleteFile(existingAvatar.Path);
                    await _fileRepository.DeleteImageFileAsync(existingAvatar.Id);
                }
            }

            // Определяем размеры изображения и сохраняем файл
            int? width = null;
            int? height = null;
            MemoryStream memoryStream = new MemoryStream();
            
            try
            {
                // Создаем копию потока для чтения размеров (так как поток может быть использован только один раз)
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var image = Image.Load(memoryStream))
                {
                    width = image.Width;
                    height = image.Height;
                }

                // Сбрасываем позицию для сохранения файла
                memoryStream.Position = 0;
            }
            catch
            {
                // Если не удалось определить размеры, продолжаем без них
                memoryStream.Position = 0;
            }

            // Сохраняем файл через FileStorageService
            var blobIdOrPath = await _fileStorageService.SaveImageFileAsync(memoryStream, fileName, "image");
            
            // Освобождаем memoryStream
            memoryStream.Dispose();

            // Создаем запись в БД
            var imageFile = new ImageFile
            {
                Path = blobIdOrPath,
                FileName = fileName,
                MimeType = contentType,
                Width = width,
                Height = height,
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            var savedFile = await _fileRepository.AddImageFileAsync(imageFile);

            // Если это аватар, обновляем пользователя
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user != null)
                {
                    user.AvatarFileId = savedFile.Id;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                }
            }

            return savedFile;
        }

        public async Task<ImageFile?> GetImageFileByIdAsync(ulong id)
        {
            return await _fileRepository.GetImageFileByIdAsync(id);
        }

        public async Task<IEnumerable<ImageFile>> GetProductFilesAsync(ulong productId)
        {
            return await _fileRepository.GetFilesByProductIdAsync(productId);
        }

        public async Task<ImageFile?> GetUserAvatarAsync(ulong userId)
        {
            return await _fileRepository.GetUserAvatarAsync(userId);
        }

        public async Task DeleteImageFileAsync(ulong id)
        {
            var file = await _fileRepository.GetImageFileByIdAsync(id);
            if (file != null)
            {
                await _fileStorageService.DeleteFile(file.Path);
                await _fileRepository.DeleteImageFileAsync(id);
            }
        }

        public string GetFileUrl(string path)
        {
            // Используем Firebase Storage публичный URL для прямой отдачи на фронт
            return _fileStorageService.GetPublicUrl(path);
        }
    }
}
