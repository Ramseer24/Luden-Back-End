using Microsoft.AspNetCore.Http;

namespace Application.DTOs.ProductDTOs
{
    public class UploadProductFilesBulkDTO
    {
        public List<IFormFile> Files { get; set; }
    }
}
