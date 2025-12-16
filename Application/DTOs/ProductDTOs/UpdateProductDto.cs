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

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public int? DiscountPercentage { get; set; }

        public ulong? RegionId { get; set; }

        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Category { get; set; }
    }
}

