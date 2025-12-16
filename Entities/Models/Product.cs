using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class Product : IEntity
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public ulong RegionId { get; set; }
        public ulong? CoverFileId { get; set; }
        public int DiscountPercentage { get; set; }
        public int SalesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // New fields
        public string Developer { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string Category { get; set; } = string.Empty;

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public Region? Region { get; set; }
        
        [JsonIgnore]
        public ICollection<BillItem>? BillItems { get; set; }
        
        [JsonIgnore]
        public ICollection<ImageFile>? Files { get; set; }
        
        [JsonIgnore]
        public ImageFile? CoverFile { get; set; }
        
        [JsonIgnore]
        public ICollection<License>? Licenses { get; set; }
    }
}
