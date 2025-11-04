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
        private readonly string _uploadPath;
        private readonly string _baseUrl;

        public FileService(
            IFileRepository fileRepository,
            IUserRepository userRepository) : base(fileRepository)
        {
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            // Путь для загрузки файлов (можно вынести в конфигурацию)
            _uploadPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            _baseUrl = "/uploads"; // Базовый URL для доступа к файлам

            // Создаем директорию если её нет
            if (!System.IO.Directory.Exists(_uploadPath))
            {
                System.IO.Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<PhotoFile> UploadUserAvatarAsync(int userId, Stream fileStream, string fileName, string contentType, long fileSize)
        {
            // Проверяем существование пользователя
            var user = await _userRepository.GetByIdAsync((ulong)userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Удаляем старый аватар если есть
            var existingAvatar = await _fileRepository.GetUserAvatarAsync(userId);
            if (existingAvatar != null)
            {
                // Удаляем физический файл
                DeletePhysicalFile(existingAvatar.Path);
                await _fileRepository.DeletePhotoFileAsync((int)existingAvatar.Id);
            }

            // Сохраняем новый файл
            var fileExt = System.IO.Path.GetExtension(fileName);
            var newFileName = $"avatar_{userId}_{Guid.NewGuid()}{fileExt}";
            var relativePath = System.IO.Path.Combine("avatars", newFileName);
            var fullPath = System.IO.Path.Combine(_uploadPath, "avatars", newFileName);

            // Создаем директорию для аватаров
            var avatarsDir = System.IO.Path.Combine(_uploadPath, "avatars");
            if (!System.IO.Directory.Exists(avatarsDir))
            {
                System.IO.Directory.CreateDirectory(avatarsDir);
            }

            // Сохраняем файл
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            // Создаем запись в БД
            var photoFile = new PhotoFile
            {
                Path = relativePath,
                FileName = fileName,
                MimeType = contentType,
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow,
                FileCategory = "Photo",
                UserId = userId
            };

            var savedFile = await _fileRepository.AddPhotoFileAsync(photoFile);

            // Обновляем пользователя
            user.AvatarFileId = (int)savedFile.Id;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return savedFile;
        }

        public async Task<ProductFile> UploadProductFileAsync(int productId, Stream fileStream, string fileName, string contentType, long fileSize, string fileType)
        {
            // Сохраняем файл
            var fileExt = System.IO.Path.GetExtension(fileName);
            var newFileName = $"product_{productId}_{Guid.NewGuid()}{fileExt}";
            var relativePath = System.IO.Path.Combine("products", newFileName);
            var fullPath = System.IO.Path.Combine(_uploadPath, "products", newFileName);

            // Создаем директорию для файлов продуктов
            var productsDir = System.IO.Path.Combine(_uploadPath, "products");
            if (!System.IO.Directory.Exists(productsDir))
            {
                System.IO.Directory.CreateDirectory(productsDir);
            }

            // Сохраняем файл
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            // Создаем запись в БД
            var productFile = new ProductFile
            {
                Path = relativePath,
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

        public async Task<PhotoFile?> GetPhotoFileByIdAsync(int id)
        {
            return await _fileRepository.GetPhotoFileByIdAsync(id);
        }

        public async Task<ProductFile?> GetProductFileByIdAsync(int id)
        {
            return await _fileRepository.GetProductFileByIdAsync(id);
        }

        public async Task<IEnumerable<ProductFile>> GetProductFilesAsync(int productId)
        {
            return await _fileRepository.GetFilesByProductIdAsync(productId);
        }

        public async Task<PhotoFile?> GetUserAvatarAsync(int userId)
        {
            return await _fileRepository.GetUserAvatarAsync(userId);
        }

        public async Task DeletePhotoFileAsync(int id)
        {
            var file = await _fileRepository.GetPhotoFileByIdAsync(id);
            if (file != null)
            {
                DeletePhysicalFile(file.Path);
                await _fileRepository.DeletePhotoFileAsync(id);
            }
        }

        public async Task DeleteProductFileAsync(int id)
        {
            var file = await _fileRepository.GetProductFileByIdAsync(id);
            if (file != null)
            {
                DeletePhysicalFile(file.Path);
                await _fileRepository.DeleteProductFileAsync(id);
            }
        }

        public string GetFileUrl(string path)
        {
            return $"{_baseUrl}/{path.Replace("\\", "/")}";
        }

        private void DeletePhysicalFile(string relativePath)
        {
            var fullPath = System.IO.Path.Combine(_uploadPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
