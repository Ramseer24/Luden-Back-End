using Entities.Models;
using FileEntity = Entities.Models.File;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IFileRepository : IGenericRepository<FileEntity>
    {
        Task<PhotoFile?> GetPhotoFileByIdAsync(int id);
        Task<ProductFile?> GetProductFileByIdAsync(int id);
        Task<IEnumerable<ProductFile>> GetFilesByProductIdAsync(int productId);
        Task<PhotoFile?> GetUserAvatarAsync(int userId);
        Task<PhotoFile> AddPhotoFileAsync(PhotoFile photoFile);
        Task<ProductFile> AddProductFileAsync(ProductFile productFile);
        Task DeletePhotoFileAsync(int id);
        Task DeleteProductFileAsync(int id);
    }
}
