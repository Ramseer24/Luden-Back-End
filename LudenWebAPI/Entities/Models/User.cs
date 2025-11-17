using Entities.Enums;
using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class User : IEntity
    {
        public ulong Id { get; set; }
        public string? GoogleId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public decimal BonusPoints { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Фото аватара
        public ulong? AvatarFileId { get; set; }

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public ImageFile? AvatarFile { get; set; }

        [JsonIgnore]
        public ICollection<Bill>? Bills { get; set; }
    }
}
