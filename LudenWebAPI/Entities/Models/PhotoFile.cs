using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    // Фото файлы (аватары пользователей)
    public class PhotoFile : File
    {
        public int? Width { get; set; }
        public int? Height { get; set; }

        // Навигационное свойство - пользователь, которому принадлежит аватар
        public ulong? UserId { get; set; }
        public User? User { get; set; }
    }
}
