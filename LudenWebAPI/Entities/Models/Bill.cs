
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    [Table("Bills")]
    public class Bill : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        // Навигационные свойства
        public ICollection<BillItem> BillItems { get; set; }
    }
}
