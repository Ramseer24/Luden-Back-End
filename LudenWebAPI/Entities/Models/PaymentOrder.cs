using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class PaymentOrder : IEntity
    {
        public ulong Id { get; set; }
        public string ProviderTransactionId { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public bool Success { get; set; }
        public decimal AmountInMinorUnits { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int TokensAmount { get; set; }
        public DateTime DeliveredAt { get; set; }
        public ulong UserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационное свойство (игнорируется при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public User? User { get; set; }
    }
}
