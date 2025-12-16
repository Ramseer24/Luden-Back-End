using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class StripeOptions : IEntity
    {
        public string ClientId { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public bool UseSandbox { get; set; } = true;
        public ulong Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
