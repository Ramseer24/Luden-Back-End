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
        public async Task<ActionResult<ProductFileDto>> UploadProductFile(
            ulong productId,
            [FromForm] UploadProductFileDTO dto)
        {
            try
            {
                if (dto.File == null || dto.File.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                // Проверка типа файла (только изображения)
                var allAllowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

                if (!allAllowedTypes.Contains(dto.File.ContentType.ToLower()))
                {
                    return BadRequest("Invalid file type. Allowed: JPEG, PNG, GIF, WebP");
                }

                // Проверка размера (макс 100MB для архивов, 10MB для изображений)
                var maxSize = allAllowedTypes.Contains(dto.File.ContentType.ToLower()) ? 10 * 1024 * 1024 : 100 * 1024 * 1024;
                if (dto.File.Length > maxSize)
                {
                    return BadRequest("File size must not exceed 10MB");
                }

                using (var stream = dto.File.OpenReadStream())
                {
                    var imageFile = await _fileService.UploadImageAsync(
                        null,
                        productId,
                        stream,
                        dto.File.FileName,
                        dto.File.ContentType,
                        dto.File.Length);

                    return Ok(new
                    {
                        id = imageFile.Id,
                        fileName = imageFile.FileName,
                        mimeType = imageFile.MimeType,
                        width = imageFile.Width,
                        height = imageFile.Height,
                        productId = imageFile.ProductId,
                        url = _fileService.GetFileUrl(imageFile.Path),
                        createdAt = imageFile.CreatedAt
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
        public async Task<ActionResult<IEnumerable<ProductFileDto>>> GetProductFiles(ulong productId)
        {
            try
            {
                var files = await _fileService.GetFilesByProductIdAsync(productId);
                var result = files.Select(f => new
                {
                    id = f.Id,
                    fileName = f.FileName,
                    mimeType = f.MimeType,
                    width = f.Width,
                    height = f.Height,
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
        public async Task<ActionResult<ProductFileDto>> GetProductFile(ulong fileId)
        {
            try
            {
                var file = await _fileService.GetImageFileByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound("File not found");
                }

                return Ok(new
                {
                    id = file.Id,
                    fileName = file.FileName,
                    mimeType = file.MimeType,
                    width = file.Width,
                    height = file.Height,
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
                var file = await _fileService.GetImageFileByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound("File not found");
                }

                await _fileService.DeleteAsync(fileId);
                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/File/photo/{photoId} - Получение информации о фото
        [HttpGet("photo/{photoId}")]
        public async Task<ActionResult<ImageFile>> GetPhotoFile(ulong photoId)
        {
            try
            {
                var photo = await _fileService.GetImageFileByIdAsync(photoId);
                if (photo == null)
                {
                    return NotFound("Photo not found");
                }

                return Ok(new
                {
                    id = photo.Id,
                    fileName = photo.FileName,
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
                var photo = await _fileService.GetImageFileByIdAsync(photoId);
                if (photo == null)
                {
                    return NotFound("Photo not found");
                }

                await _fileService.DeleteAsync(photoId);
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
                            var imageFile = await _fileService.UploadImageAsync(
                                null,
                                productId,
                                stream,
                                file.FileName,
                                file.ContentType,
                                file.Length);

                            uploadedFiles.Add(new
                            {
                                id = imageFile.Id,
                                fileName = imageFile.FileName,
                                width = imageFile.Width,
                                height = imageFile.Height,
                                url = _fileService.GetFileUrl(imageFile.Path)
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
