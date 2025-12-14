using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IFileRepository : IGenericRepository<ImageFile>
    {
        Task<PhotoFile?> GetPhotoFileByIdAsync(ulong id);
        Task<ProductFile?> GetProductFileByIdAsync(ulong id);
        Task<IEnumerable<ProductFile>> GetFilesByProductIdAsync(ulong productId);
        Task<PhotoFile?> GetUserAvatarAsync(ulong userId);
        Task<PhotoFile> AddPhotoFileAsync(PhotoFile photoFile);
        Task<ProductFile> AddProductFileAsync(ProductFile productFile);
        Task DeletePhotoFileAsync(ulong id);
        Task DeleteProductFileAsync(ulong id);
    }
}
