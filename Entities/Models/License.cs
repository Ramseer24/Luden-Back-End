using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class License : IEntity
    {
        public ulong Id { get; set; }
        public ulong ProductId { get; set; }
        public ulong BillItemId { get; set; }
        public string LicenseKey { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public Product? Product { get; set; }

        [JsonIgnore]
        public BillItem? BillItem { get; set; }
    }
}
