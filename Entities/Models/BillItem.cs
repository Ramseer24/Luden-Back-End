using System.Text.Json.Serialization;

namespace Entities.Models
{
    public class BillItem : IEntity
    {
        public ulong Id { get; set; }
        public ulong BillId { get; set; }
        public ulong ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }

        // Навигационные свойства (игнорируются при сериализации для избежания циклических ссылок)
        [JsonIgnore]
        public Bill? Bill { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
