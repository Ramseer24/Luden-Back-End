using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    // Файлы продуктов (цифровые товары, скриншоты и т.д.)
    public class ProductFile : File
    {
        [Required]
        public ulong ProductId { get; set; }
        public Product Product { get; set; }

        [MaxLength(100)]
        public string? FileType { get; set; } // например: "installer", "screenshot", "manual"

        public int? DisplayOrder { get; set; } // Порядок отображения
    }
}
