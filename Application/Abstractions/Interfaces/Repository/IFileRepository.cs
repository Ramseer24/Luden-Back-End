using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IFileRepository : IGenericRepository<ImageFile>
    {
        Task<IEnumerable<ImageFile>> GetFilesByProductIdAsync(ulong productId);
        Task<ImageFile?> GetUserAvatarAsync(ulong userId);
    }
}
