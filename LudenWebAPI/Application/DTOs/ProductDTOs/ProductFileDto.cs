using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ProductDTOs
{
    public class ProductFileDto
    {
        public ulong Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string? FileType { get; set; }
        public int? DisplayOrder { get; set; }
        public string MimeType { get; set; }
        public string? Url { get; set; }
    }
}
