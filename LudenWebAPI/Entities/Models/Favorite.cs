using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class Favorite : IEntity
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public ulong ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
    }
}

