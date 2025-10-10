
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = Entities.Models.File;

namespace Entities.Models
{
    [Table("Products")]
    public class Product : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public int? RegionId { get; set; }
        public Region Region { get; set; }

        // Навигационные свойства
        public ICollection<BillItem> BillItems { get; set; }
        public ICollection<File> Files { get; set; }
        public ICollection<License> Licenses { get; set; }
    }
}
