using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Models;
using FileEntity = Entities.Models.File;

namespace Application.Services
{
    public class FileService : GenericService<FileEntity>, IFileService
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

        public async Task<PhotoFile> UploadUserAvatarAsync(ulong userId, Stream fileStream, string fileName, string contentType, long fileSize)
        {
            // Проверяем существование пользователя
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Удаляем старый аватар если есть
            var existingAvatar = await _fileRepository.GetUserAvatarAsync(userId);
            if (existingAvatar != null)
            {
                // Удаляем физический файл через FileStorageService
                await _fileStorageService.DeleteFile(existingAvatar.Path);
                await _fileRepository.DeletePhotoFileAsync(existingAvatar.Id);
            }

            // Сохраняем новый файл через FileStorageService
            // В Firebase режиме: возвращает blob ID (строка), файл хранится как base64 в Firebase Realtime Database
            // В SQLite режиме: возвращает относительный путь к файлу
            var blobIdOrPath = await _fileStorageService.SaveImageFileAsync(fileStream, fileName, "image");

            // Создаем запись в БД
            var photoFile = new PhotoFile
            {
                Path = blobIdOrPath, // В Firebase режиме это blob ID, в SQLite - путь к файлу
                FileName = fileName,
                MimeType = contentType,
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow,
                FileCategory = "Photo",
                UserId = userId
            };

            var savedFile = await _fileRepository.AddPhotoFileAsync(photoFile);

            // Обновляем пользователя
            user.AvatarFileId = savedFile.Id;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return savedFile;
        }

        public async Task<ProductFile> UploadProductFileAsync(ulong productId, Stream fileStream, string fileName, string contentType, long fileSize, string fileType)
        {
            // Сохраняем файл через FileStorageService
            // В Firebase режиме: возвращает blob ID (строка), файл хранится как base64 в Firebase Realtime Database
            // В SQLite режиме: возвращает относительный путь к файлу
            var blobIdOrPath = await _fileStorageService.SaveImageFileAsync(fileStream, fileName, "image");

            // Создаем запись в БД
            var productFile = new ProductFile
            {
                Path = blobIdOrPath, // В Firebase режиме это blob ID, в SQLite - путь к файлу
                FileName = fileName,
                MimeType = contentType,
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow,
                FileCategory = "Product",
                ProductId = productId,
                FileType = fileType
            };

            return await _fileRepository.AddProductFileAsync(productFile);
        }

        public async Task<PhotoFile?> GetPhotoFileByIdAsync(ulong id)
        {
            return await _fileRepository.GetPhotoFileByIdAsync(id);
        }

        public async Task<ProductFile?> GetProductFileByIdAsync(ulong id)
        {
            return await _fileRepository.GetProductFileByIdAsync(id);
        }

        public async Task<IEnumerable<ProductFile>> GetProductFilesAsync(ulong productId)
        {
            return await _fileRepository.GetFilesByProductIdAsync(productId);
        }

        public async Task<PhotoFile?> GetUserAvatarAsync(ulong userId)
        {
            return await _fileRepository.GetUserAvatarAsync(userId);
        }

        public async Task DeletePhotoFileAsync(ulong id)
        {
            var file = await _fileRepository.GetPhotoFileByIdAsync(id);
            if (file != null)
            {
                await _fileStorageService.DeleteFile(file.Path);
                await _fileRepository.DeletePhotoFileAsync(id);
            }
        }

        public async Task DeleteProductFileAsync(ulong id)
        {
            var file = await _fileRepository.GetProductFileByIdAsync(id);
            if (file != null)
            {
                await _fileStorageService.DeleteFile(file.Path);
                await _fileRepository.DeleteProductFileAsync(id);
            }
        }

        public string GetFileUrl(string path)
        {
            // Используем Firebase Storage публичный URL для прямой отдачи на фронт
            return _fileStorageService.GetPublicUrl(path);
        }
    }
}
