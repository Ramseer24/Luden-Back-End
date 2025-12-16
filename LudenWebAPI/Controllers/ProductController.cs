using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.ProductDTOs;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService productService, ITokenService tokenService, IUserService userService) : ControllerBase
    {


        // GET: api/Product
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                var products = await productService.GetAllProductDtosAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving products: {ex.Message}");
            }
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDto>> GetProduct(ulong id)
        {
            try
            {
                var product = await productService.GetProductDtoByIdAsync(id);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Product with ID {id} not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the product: {ex.Message}");
            }
        }

        // POST: api/Product
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            User user = await userService.GetByIdAsync(tokenService.GetUserIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", "")));
            if (user.Role != Entities.Enums.UserRole.Admin)
                return Forbid();

            try
            {
                Stream? coverStream = null;
                string? coverFileName = null;
                string? coverContentType = null;
                long? coverFileSize = null;

                // Обрабатываем файл обложки, если он передан
                if (dto.Cover != null && dto.Cover.Length > 0)
                {
                    // Проверка типа файла (только изображения)
                    var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedImageTypes.Contains(dto.Cover.ContentType.ToLower()))
                    {
                        return BadRequest("Invalid cover file type. Allowed: JPEG, PNG, GIF, WebP");
                    }

                    // Проверка размера (макс 10MB)
                    if (dto.Cover.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest("Cover file size must not exceed 10MB");
                    }

                    coverStream = dto.Cover.OpenReadStream();
                    coverFileName = dto.Cover.FileName;
                    coverContentType = dto.Cover.ContentType;
                    coverFileSize = dto.Cover.Length;
                }

                var product = await productService.CreateProductAsync(dto, coverStream, coverFileName, coverContentType, coverFileSize);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the product: {ex.Message}");
            }
        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductDto>> UpdateProduct(ulong id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = await userService.GetByIdAsync(tokenService.GetUserIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", "")));
            if (user.Role != Entities.Enums.UserRole.Admin)
                return Forbid();

            try
            {
                var product = await productService.UpdateProductAsync(id, dto);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Product with ID {id} not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the product: {ex.Message}");
            }
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(ulong id)
        {
            User user = await userService.GetByIdAsync(tokenService.GetUserIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", "")));
            if (user.Role != Entities.Enums.UserRole.Admin)
                return Forbid();

            try
            {
                var product = await productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                await productService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the product: {ex.Message}");
            }
        }

        // PUT: api/Product/{id}/cover/{coverFileId}
        [HttpPut("{id}/cover/{coverFileId}")]
        [Authorize]
        public async Task<ActionResult<ProductDto>> SetProductCover(ulong id, ulong coverFileId)
        {

            User user = await userService.GetByIdAsync(tokenService.GetUserIdFromToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", "")));
            if (user.Role != Entities.Enums.UserRole.Admin)
                return Forbid();

            try
            {
                var product = await productService.SetProductCoverAsync(id, coverFileId);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while setting product cover: {ex.Message}");
            }
        }
    }
}

