using Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class User : IEntity
    {
        [Key]
        public ulong Id { get; set; }
        public string? GoogleId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = "user";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Фото аватара
        public int? AvatarFileId { get; set; }
        public PhotoFile? AvatarFile { get; set; }

        // Навигационные свойства
        public ICollection<Bill> Bills { get; set; }
    }
}
