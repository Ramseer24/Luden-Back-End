
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    [Table("Regions")]
    public class Region : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }

        // Навигационные свойства
        public ICollection<Product> Products { get; set; }
    }
}
