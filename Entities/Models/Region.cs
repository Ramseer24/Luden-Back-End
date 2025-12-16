using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class Region : IEntity
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }
    }
}
