using Entities.Models;
using FileEntity = Entities.Models.File;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IFileService : IGenericService<FileEntity>
    {
        Task<PhotoFile> UploadUserAvatarAsync(int userId, Stream fileStream, string fileName, string contentType, long fileSize);
        Task<ProductFile> UploadProductFileAsync(int productId, Stream fileStream, string fileName, string contentType, long fileSize, string fileType);
        Task<PhotoFile?> GetPhotoFileByIdAsync(int id);
        Task<ProductFile?> GetProductFileByIdAsync(int id);
        Task<IEnumerable<ProductFile>> GetProductFilesAsync(int productId);
        Task<PhotoFile?> GetUserAvatarAsync(int userId);
        Task DeletePhotoFileAsync(int id);
        Task DeleteProductFileAsync(int id);
        string GetFileUrl(string path);
    }
}
