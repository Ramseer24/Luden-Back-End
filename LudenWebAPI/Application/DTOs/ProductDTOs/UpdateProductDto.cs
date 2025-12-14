using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProductDTOs
{
    public class UpdateProductDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be non-negative")]
        public int? Stock { get; set; }

        public ulong? RegionId { get; set; }
    }
}

