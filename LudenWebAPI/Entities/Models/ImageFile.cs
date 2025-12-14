using System.Text.Json.Serialization;

namespace Entities.Models
{
    // Единая модель для всех изображений (аватары пользователей и файлы продуктов)
    public class ImageFile : IEntity
    {
        public ulong Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public ulong? UserId { get; set; }
        public ulong? ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
    }
}

