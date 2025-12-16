using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Models;

namespace Application.Services
{
    public class FileService : GenericService<ImageFile>, IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;

        public FileService(
            IFileRepository fileRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService) : base(fileRepository)
        {
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<ImageFile> UploadImageAsync(ulong? currentFileId, ulong entityId, Stream fileStream, string fileName, string contentType, long fileSize, bool isUser = false)
        {
            // If isUser is true, entityId is userId. If false, it's productId.
            
            if (isUser)
            {
                 // Handle User Avatar logic
                 var user = await _userRepository.GetByIdAsync(entityId);
                 if (user == null)
                     throw new InvalidOperationException($"User with ID {entityId} not found");

                 // Remove old avatar
                 var existingAvatar = await _fileRepository.GetUserAvatarAsync(entityId);
                 if (existingAvatar != null)
                 {
                     await _fileStorageService.DeleteFile(existingAvatar.Path);
                     await _fileRepository.RemoveByIdAsync(existingAvatar.Id);
                 }
            }

            var blobIdOrPath = await _fileStorageService.SaveImageFileAsync(fileStream, fileName, "image");

            var imageFile = new ImageFile
            {
                Path = blobIdOrPath,
                FileName = fileName,
                MimeType = contentType,
                CreatedAt = DateTime.UtcNow,
                UserId = isUser ? entityId : (ulong?)null,
                ProductId = !isUser ? entityId : (ulong?)null
            };
            
            await _fileRepository.AddAsync(imageFile);
            
            if (isUser)
            {
                 var user = await _userRepository.GetByIdAsync(entityId);
                 if (user != null)
                 {
                     user.AvatarFileId = imageFile.Id;
                     user.UpdatedAt = DateTime.UtcNow;
                     await _userRepository.UpdateAsync(user);
                 }
            }

            return imageFile;
        }

        public async Task<ImageFile?> GetImageFileByIdAsync(ulong id)
        {
            return await _fileRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ImageFile>> GetFilesByProductIdAsync(ulong productId)
        {
            return await _fileRepository.GetFilesByProductIdAsync(productId);
        }

        public async Task<ImageFile?> GetUserAvatarAsync(ulong userId)
        {
            return await _fileRepository.GetUserAvatarAsync(userId);
        }

        public string GetFileUrl(string path)
        {
            return _fileStorageService.GetPublicUrl(path);
        }
    }
}
