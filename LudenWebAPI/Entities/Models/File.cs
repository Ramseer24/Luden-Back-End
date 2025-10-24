using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    // Базовый класс для всех файлов
    [Table("Files")]
    public abstract class File : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Path { get; set; }

        [Required]
        [MaxLength(100)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(50)]
        public string MimeType { get; set; }

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Discriminator для TPH (Table Per Hierarchy)
        [Required]
        [MaxLength(50)]
        public string FileCategory { get; set; }
    }
}
