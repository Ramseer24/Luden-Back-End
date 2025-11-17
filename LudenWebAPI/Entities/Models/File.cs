using System.Text.Json.Serialization;

namespace Entities.Models
{
    // Базовый класс для всех файлов
    public abstract class File : IEntity
    {
        public ulong Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Discriminator для определения типа файла
        public string FileCategory { get; set; }
    }
}
