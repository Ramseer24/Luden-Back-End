using System.Text.Json.Serialization;

namespace Entities.Models
{
    // Фото файлы (аватары пользователей)
    public class PhotoFile : File
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public ulong? UserId { get; set; }

        // Навигационное свойство (игнорируется при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public User? User { get; set; }
    }
}
