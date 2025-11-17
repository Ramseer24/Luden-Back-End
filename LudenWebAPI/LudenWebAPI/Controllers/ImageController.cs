using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{
    [Route("api/blob")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IFileRepository _fileRepository;
        private readonly IFileStorageService _fileStorageService;

        public ImageController(IFileRepository fileRepository, IFileStorageService fileStorageService)
        {
            _fileRepository = fileRepository;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Retrieves an image file by its ID.
        /// Path содержит путь к файлу в GitHub репозитории (например: uploads/image/2024/12/guid.jpg)
        /// </summary>
        /// <param name="id">The ID of the image to retrieve.</param>
        /// <param name="redirect">Если true, перенаправляет на прямую ссылку raw.githubusercontent.com</param>
        /// <param name="dataUri">Если true, возвращает data URI вместо файла (для прямого использования в img src)</param>
        /// <returns>Image file with appropriate content type.</returns>
        /// <response code="200">Image retrieved successfully.</response>
        /// <response code="302">Redirect to GitHub raw URL.</response>
        /// <response code="404">Image not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImageById([FromRoute] ulong id, [FromQuery] bool redirect = false, [FromQuery] bool dataUri = false)
        {
            var file = await _fileRepository.GetImageFileByIdAsync(id);

            if (file == null)
            {
                return NotFound($"Image file with ID '{id}' not found");
            }

            // Если redirect=true, перенаправляем на прямую ссылку GitHub
            if (redirect)
            {
                var publicUrl = _fileStorageService.GetPublicUrl(file.Path);
                return Redirect(publicUrl);
            }

            // Получаем файл из GitHub репозитория
            byte[] imageBytes;
            try
            {
                imageBytes = await _fileStorageService.GetFile(file.Path);
            }
            catch (FileNotFoundException)
            {
                return NotFound($"Image file with ID '{id}' not found");
            }

            string contentType = file.MimeType ?? GetContentType(file.FileName);

            // Если dataUri=true, возвращаем data URI для прямого использования в img src
            if (dataUri)
            {
                string base64 = Convert.ToBase64String(imageBytes);
                string dataUriString = $"data:{contentType};base64,{base64}";
                return Content(dataUriString, "text/plain");
            }

            // Иначе возвращаем файл с правильным Content-Type (проксирование через API)
            return File(imageBytes, contentType);
        }

        private static string GetContentType(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                _ => "image/jpeg" // Default to JPEG if unknown
            };
        }
    }
}

