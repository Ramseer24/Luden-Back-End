using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class PaymentOrder : IEntity
    {
        public string ProviderTransactionId { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public bool Success { get; set; }
        public decimal AmountInMinorUnits { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int TokensAmount { get; set; }
        public DateTime DeliveredAt { get; set; }
        public ulong UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime? UpdatedAt { get; set; }
        public ulong Id { get; set; }
    }
}
