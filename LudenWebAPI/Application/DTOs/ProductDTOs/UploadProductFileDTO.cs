using Microsoft.AspNetCore.Http;

namespace Application.DTOs.ProductDTOs
{
    public class UploadProductFileDTO
    {
        public IFormFile File { get; set; }
    }
}
