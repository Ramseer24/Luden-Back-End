using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IFavoriteRepository : IGenericRepository<Favorite>
    {
        Task<IEnumerable<Favorite>> GetFavoritesByUserIdAsync(ulong userId);
        Task<Favorite?> GetFavoriteByUserAndProductAsync(ulong userId, ulong productId);
        Task<bool> ExistsByUserAndProductAsync(ulong userId, ulong productId);
    }
}

