using Application.DTOs.ProductDTOs;

namespace Application.DTOs.FavoriteDTOs
{
    public class FavoriteDto
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public ProductDto Product { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

