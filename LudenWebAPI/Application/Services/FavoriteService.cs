using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.FavoriteDTOs;
using Entities.Models;

namespace Application.Services
{
    public class FavoriteService : GenericService<Favorite>, IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IGenericRepository<Product> _productRepository;

        public FavoriteService(
            IFavoriteRepository favoriteRepository,
            IProductService productService,
            IUserService userService,
            IGenericRepository<Product> productRepository) : base(favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
            _productService = productService;
            _userService = userService;
            _productRepository = productRepository;
        }

        public async Task<FavoriteDto> AddFavoriteAsync(ulong userId, ulong productId)
        {
            // Проверяем существование пользователя
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' does not exist");
            }

            // Проверяем существование продукта
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID '{productId}' does not exist");
            }

            // Проверяем, не добавлен ли уже продукт в избранное
            var existingFavorite = await _favoriteRepository.GetFavoriteByUserAndProductAsync(userId, productId);
            if (existingFavorite != null)
            {
                throw new InvalidOperationException("Product is already in favorites");
            }

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _favoriteRepository.AddAsync(favorite);

            // Получаем ProductDto для возврата
            var productDto = await _productService.GetProductDtoByIdAsync(productId);

            return new FavoriteDto
            {
                Id = favorite.Id,
                UserId = userId,
                Product = productDto,
                CreatedAt = favorite.CreatedAt
            };
        }

        public async Task RemoveFavoriteAsync(ulong userId, ulong productId)
        {
            var favorite = await _favoriteRepository.GetFavoriteByUserAndProductAsync(userId, productId);
            if (favorite == null)
            {
                throw new KeyNotFoundException("Favorite not found");
            }

            await _favoriteRepository.RemoveAsync(favorite);
        }

        public async Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(ulong userId)
        {
            var favorites = await _favoriteRepository.GetFavoritesByUserIdAsync(userId);
            var favoriteDtos = new List<FavoriteDto>();

            foreach (var favorite in favorites)
            {
                try
                {
                    var productDto = await _productService.GetProductDtoByIdAsync(favorite.ProductId);
                    favoriteDtos.Add(new FavoriteDto
                    {
                        Id = favorite.Id,
                        UserId = userId,
                        Product = productDto,
                        CreatedAt = favorite.CreatedAt
                    });
                }
                catch (KeyNotFoundException)
                {
                    // Если продукт был удален, пропускаем его
                    continue;
                }
            }

            return favoriteDtos;
        }

        public async Task<bool> IsFavoriteAsync(ulong userId, ulong productId)
        {
            return await _favoriteRepository.ExistsByUserAndProductAsync(userId, productId);
        }
    }
}

