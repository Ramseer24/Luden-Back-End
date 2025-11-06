using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Entities.Models
{
    [Table("Products")]
    public class Product : IEntity
    {
        [Key]
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public ulong RegionId { get; set; }
        public Region Region { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        [JsonIgnore]
        public ICollection<BillItem> BillItems { get; set; }
        public ICollection<ProductFile> Files { get; set; }
        public ICollection<License> Licenses { get; set; }
    }
}
