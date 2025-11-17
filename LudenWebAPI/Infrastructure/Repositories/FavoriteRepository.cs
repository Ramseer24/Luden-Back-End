using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;

namespace Infrastructure.Repositories
{
    public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
    {
        private readonly FirebaseRepository _firebaseRepo;

        public FavoriteRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
            _firebaseRepo = firebaseRepo;
        }

        public async Task<IEnumerable<Favorite>> GetFavoritesByUserIdAsync(ulong userId)
        {
            var favorites = new List<Favorite>();

            await _firebaseRepo.GetAsync<Dictionary<string, Favorite>>(
                "favorites",
                new FirebaseConsoleListener<Dictionary<string, Favorite>>(data =>
                {
                    if (data != null)
                    {
                        foreach (var f in data.Values)
                        {
                            if (f.UserId == userId)
                                favorites.Add(f);
                        }
                    }
                })
            );

            return favorites.OrderByDescending(f => f.CreatedAt);
        }

        public async Task<Favorite?> GetFavoriteByUserAndProductAsync(ulong userId, ulong productId)
        {
            var favorites = await GetFavoritesByUserIdAsync(userId);
            return favorites.FirstOrDefault(f => f.ProductId == productId);
        }

        public async Task<bool> ExistsByUserAndProductAsync(ulong userId, ulong productId)
        {
            var favorite = await GetFavoriteByUserAndProductAsync(userId, productId);
            return favorite != null;
        }
    }
}

