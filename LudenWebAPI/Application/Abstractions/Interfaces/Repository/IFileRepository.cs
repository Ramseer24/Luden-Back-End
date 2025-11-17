using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IFileRepository : IGenericRepository<ImageFile>
    {
        Task<ImageFile?> GetImageFileByIdAsync(ulong id);
        Task<IEnumerable<ImageFile>> GetFilesByProductIdAsync(ulong productId);
        Task<ImageFile?> GetUserAvatarAsync(ulong userId);
        Task<ImageFile> AddImageFileAsync(ImageFile imageFile);
        Task DeleteImageFileAsync(ulong id);
    }
}
