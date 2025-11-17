using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IFileService : IGenericService<ImageFile>
    {
        Task<ImageFile> UploadImageAsync(ulong? userId, ulong? productId, Stream fileStream, string fileName, string contentType, long fileSize);
        Task<ImageFile?> GetImageFileByIdAsync(ulong id);
        Task<IEnumerable<ImageFile>> GetProductFilesAsync(ulong productId);
        Task<ImageFile?> GetUserAvatarAsync(ulong userId);
        Task DeleteImageFileAsync(ulong id);
        string GetFileUrl(string path);
    }
}
