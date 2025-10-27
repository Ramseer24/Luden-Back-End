using Application.Abstractions.Interfaces.Services;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // POST: api/File/product/{productId} - Загрузка файла для продукта
        [HttpPost("product/{productId}")]
        public async Task<ActionResult<ProductFile>> UploadProductFile(
            int productId,
            [FromForm] IFormFile file,
            [FromForm] string fileType = "screenshot",
            [FromForm] int displayOrder = 0)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                // Проверка типа файла (разрешаем изображения и архивы)
                var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                var allowedArchiveTypes = new[] { "application/zip", "application/x-zip-compressed", "application/x-rar-compressed", "application/octet-stream" };
                var allAllowedTypes = allowedImageTypes.Concat(allowedArchiveTypes).ToArray();

                if (!allAllowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest("Invalid file type. Allowed: images (JPEG, PNG, GIF, WebP) and archives (ZIP, RAR)");
                }

                // Проверка размера (макс 100MB для архивов, 10MB для изображений)
                var maxSize = allowedImageTypes.Contains(file.ContentType.ToLower()) ? 10 * 1024 * 1024 : 100 * 1024 * 1024;
                if (file.Length > maxSize)
                {
                    return BadRequest($"File size must not exceed {maxSize / (1024 * 1024)}MB");
                }

                using (var stream = file.OpenReadStream())
                {
                    var productFile = await _fileService.UploadProductFileAsync(
                        productId,
                        stream,
                        file.FileName,
                        file.ContentType,
                        file.Length,
                        fileType);

                    // Устанавливаем порядок отображения
                    productFile.DisplayOrder = displayOrder;

                    return Ok(new
                    {
                        id = productFile.Id,
                        fileName = productFile.FileName,
                        fileType = productFile.FileType,
                        fileSize = productFile.FileSize,
                        mimeType = productFile.MimeType,
                        displayOrder = productFile.DisplayOrder,
                        url = _fileService.GetFileUrl(productFile.Path),
                        createdAt = productFile.CreatedAt
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/File/product/{productId} - Получение всех файлов продукта
        [HttpGet("product/{productId}")]
        [AllowAnonymous] // Разрешаем анонимный доступ для просмотра товаров
        public async Task<ActionResult<IEnumerable<ProductFile>>> GetProductFiles(int productId)
        {
            try
            {
                var files = await _fileService.GetProductFilesAsync(productId);
                var result = files.Select(f => new
                {
                    id = f.Id,
                    fileName = f.FileName,
                    fileType = f.FileType,
                    fileSize = f.FileSize,
                    mimeType = f.MimeType,
                    displayOrder = f.DisplayOrder,
                    url = _fileService.GetFileUrl(f.Path),
                    createdAt = f.CreatedAt
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/File/product/file/{fileId} - Получение информации о конкретном файле продукта
        [HttpGet("product/file/{fileId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductFile>> GetProductFile(int fileId)
        {
            try
            {
                var file = await _fileService.GetProductFileByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound("File not found");
                }

                return Ok(new
                {
                    id = file.Id,
                    fileName = file.FileName,
                    fileType = file.FileType,
                    fileSize = file.FileSize,
                    mimeType = file.MimeType,
                    displayOrder = file.DisplayOrder,
                    productId = file.ProductId,
                    url = _fileService.GetFileUrl(file.Path),
                    createdAt = file.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE: api/File/product/{fileId} - Удаление файла продукта
        [HttpDelete("product/{fileId}")]
        public async Task<ActionResult> DeleteProductFile(int fileId)
        {
            try
            {
                var file = await _fileService.GetProductFileByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound("File not found");
                }

                await _fileService.DeleteProductFileAsync(fileId);
                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/File/photo/{photoId} - Получение информации о фото
        [HttpGet("photo/{photoId}")]
        public async Task<ActionResult<PhotoFile>> GetPhotoFile(int photoId)
        {
            try
            {
                var photo = await _fileService.GetPhotoFileByIdAsync(photoId);
                if (photo == null)
                {
                    return NotFound("Photo not found");
                }

                return Ok(new
                {
                    id = photo.Id,
                    fileName = photo.FileName,
                    fileSize = photo.FileSize,
                    mimeType = photo.MimeType,
                    width = photo.Width,
                    height = photo.Height,
                    userId = photo.UserId,
                    url = _fileService.GetFileUrl(photo.Path),
                    createdAt = photo.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE: api/File/photo/{photoId} - Удаление фото
        [HttpDelete("photo/{photoId}")]
        public async Task<ActionResult> DeletePhotoFile(int photoId)
        {
            try
            {
                var photo = await _fileService.GetPhotoFileByIdAsync(photoId);
                if (photo == null)
                {
                    return NotFound("Photo not found");
                }

                await _fileService.DeletePhotoFileAsync(photoId);
                return Ok(new { message = "Photo deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/File/product/{productId}/bulk - Массовая загрузка файлов для продукта
        [HttpPost("product/{productId}/bulk")]
        public async Task<ActionResult> UploadProductFilesBulk(
            int productId,
            [FromForm] List<IFormFile> files,
            [FromForm] string fileType = "screenshot")
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No files provided");
                }

                var uploadedFiles = new List<object>();
                int displayOrder = 0;

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                        if (!allowedTypes.Contains(file.ContentType.ToLower()))
                        {
                            continue; // Пропускаем неподдерживаемые типы
                        }

                        if (file.Length > 10 * 1024 * 1024)
                        {
                            continue; // Пропускаем слишком большие файлы
                        }

                        using (var stream = file.OpenReadStream())
                        {
                            var productFile = await _fileService.UploadProductFileAsync(
                                productId,
                                stream,
                                file.FileName,
                                file.ContentType,
                                file.Length,
                                fileType);

                            productFile.DisplayOrder = displayOrder++;

                            uploadedFiles.Add(new
                            {
                                id = productFile.Id,
                                fileName = productFile.FileName,
                                fileType = productFile.FileType,
                                url = _fileService.GetFileUrl(productFile.Path)
                            });
                        }
                    }
                }

                return Ok(new
                {
                    message = $"{uploadedFiles.Count} files uploaded successfully",
                    files = uploadedFiles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
