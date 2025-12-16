using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IFileService : IGenericService<ImageFile>
    {
        Task<ImageFile> UploadImageAsync(ulong? currentFileId, ulong entityId, Stream fileStream, string fileName, string contentType, long fileSize, bool isUser = false);
        Task<ImageFile?> GetImageFileByIdAsync(ulong id);
        Task<IEnumerable<ImageFile>> GetFilesByProductIdAsync(ulong productId);
        Task<ImageFile?> GetUserAvatarAsync(ulong userId);
        string GetFileUrl(string path);
    }
}
