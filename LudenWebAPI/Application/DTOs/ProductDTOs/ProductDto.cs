
using Entities.Models;

namespace Application.DTOs.ProductDTOs
{
    public class ProductDto
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int? RegionId { get; set; }
        public Region Region { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ProductFileDto> Files { get; set; }
        public ICollection<License> Licenses { get; set; }
    }
}
