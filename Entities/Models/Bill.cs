using Entities.Enums;
using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class Bill : IEntity
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public string Currency { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public ICollection<BillItem>? BillItems { get; set; }
    }
}
