using Microsoft.AspNetCore.Http;

namespace Application.DTOs.ProductDTOs
{
    public class UploadProductFilesBulkDTO
    {
        public required List<IFormFile> Files { get; set; }
        public string FileType { get; set; } = "screenshot";
    }
}

