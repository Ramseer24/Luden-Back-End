using Microsoft.AspNetCore.Http;

namespace Application.DTOs.ProductDTOs
{
    public class UploadProductFileDTO
    {
        public required IFormFile File { get; set; }
        public string FileType { get; set; } = "screenshot";
        public int DisplayOrder { get; set; } = 0;
    }
}

