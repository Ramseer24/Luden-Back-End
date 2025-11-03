using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Entities.Models
{
    [Table("Bill_Items")]
    public class BillItem : IEntity
    {
        [Key]
        public ulong Id { get; set; }

        public int BillId { get; set; }
        [JsonIgnore]
        public Bill Bill { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}
