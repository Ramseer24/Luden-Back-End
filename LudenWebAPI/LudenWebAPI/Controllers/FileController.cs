using Application.Abstractions.Interfaces.Services;
using Application.DTOs.ProductDTOs;
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
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductFile>> UploadProductFile(
            ulong productId,
            [FromForm] UploadProductFileDTO dto)
        {
            try
            {
                if (dto.File == null || dto.File.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                // Проверка типа файла (разрешаем изображения и архивы)
                var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                var allowedArchiveTypes = new[] { "application/zip", "application/x-zip-compressed", "application/x-rar-compressed", "application/octet-stream" };
                var allAllowedTypes = allowedImageTypes.Concat(allowedArchiveTypes).ToArray();

                if (!allAllowedTypes.Contains(dto.File.ContentType.ToLower()))
                {
                    return BadRequest("Invalid file type. Allowed: images (JPEG, PNG, GIF, WebP) and archives (ZIP, RAR)");
                }

                // Проверка размера (макс 100MB для архивов, 10MB для изображений)
                var maxSize = allowedImageTypes.Contains(dto.File.ContentType.ToLower()) ? 10 * 1024 * 1024 : 100 * 1024 * 1024;
                if (dto.File.Length > maxSize)
                {
                    return BadRequest($"File size must not exceed {maxSize / (1024 * 1024)}MB");
                }

                using (var stream = dto.File.OpenReadStream())
                {
                    var productFile = await _fileService.UploadProductFileAsync(
                        productId,
                        stream,
                        dto.File.FileName,
                        dto.File.ContentType,
                        dto.File.Length,
                        dto.FileType);

                    // Устанавливаем порядок отображения
                    productFile.DisplayOrder = dto.DisplayOrder;

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
        public async Task<ActionResult<IEnumerable<ProductFile>>> GetProductFiles(ulong productId)
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
        public async Task<ActionResult<ProductFile>> GetProductFile(ulong fileId)
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
        public async Task<ActionResult> DeleteProductFile(ulong fileId)
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
        public async Task<ActionResult<PhotoFile>> GetPhotoFile(ulong photoId)
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
        public async Task<ActionResult> DeletePhotoFile(ulong photoId)
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
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UploadProductFilesBulk(
            ulong productId,
            [FromForm] UploadProductFilesBulkDTO dto)
        {
            try
            {
                if (dto.Files == null || dto.Files.Count == 0)
                {
                    return BadRequest("No files provided");
                }

                var uploadedFiles = new List<object>();
                int displayOrder = 0;

                foreach (var file in dto.Files)
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
                                dto.FileType);

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
