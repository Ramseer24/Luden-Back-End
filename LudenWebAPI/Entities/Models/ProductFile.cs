using System.Text.Json.Serialization;

namespace Entities.Models
{
    // Файлы продуктов (цифровые товары, скриншоты и т.д.)
    public class ProductFile : File
    {
        public ulong ProductId { get; set; }
        public string? FileType { get; set; } // например: "installer", "screenshot", "manual"
        public int? DisplayOrder { get; set; } // Порядок отображения

        // Навигационное свойство (игнорируется при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
