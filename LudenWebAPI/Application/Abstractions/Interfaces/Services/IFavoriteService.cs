using Application.DTOs.FavoriteDTOs;
using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IFavoriteService : IGenericService<Favorite>
    {
        Task<FavoriteDto> AddFavoriteAsync(ulong userId, ulong productId);
        Task RemoveFavoriteAsync(ulong userId, ulong productId);
        Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(ulong userId);
        Task<bool> IsFavoriteAsync(ulong userId, ulong productId);
    }
}

