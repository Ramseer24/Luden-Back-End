using Entities.Models;
using FileEntity = Entities.Models.File;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IFileService : IGenericService<FileEntity>
    {
        Task<PhotoFile> UploadUserAvatarAsync(ulong userId, Stream fileStream, string fileName, string contentType, long fileSize);
        Task<ProductFile> UploadProductFileAsync(ulong productId, Stream fileStream, string fileName, string contentType, long fileSize, string fileType);
        Task<PhotoFile?> GetPhotoFileByIdAsync(ulong id);
        Task<ProductFile?> GetProductFileByIdAsync(ulong id);
        Task<IEnumerable<ProductFile>> GetProductFilesAsync(ulong productId);
        Task<PhotoFile?> GetUserAvatarAsync(ulong userId);
        Task DeletePhotoFileAsync(ulong id);
        Task DeleteProductFileAsync(ulong id);
        string GetFileUrl(string path);
    }
}
