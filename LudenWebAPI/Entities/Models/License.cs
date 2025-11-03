using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    [Table("Licenses")]
    public class License : IEntity
    {
        [Key]
        public ulong Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int BillItemId { get; set; }
        public BillItem BillItem { get; set; }
        public string LicenseKey { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
