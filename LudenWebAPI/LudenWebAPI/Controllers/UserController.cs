using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.ProductDTOs;
using Application.DTOs.UserDTOs;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LudenWebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IFileService _fileService;

        public UserController(IUserService userService, ITokenService tokenService, IFileService fileService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _fileService = fileService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                });
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving users: {ex.Message}");
            }
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(ulong id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the user: {ex.Message}");
            }
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(ulong id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                await _userService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        // PUT: api/User/update - обновление информации пользователя (включая аватар)
        [HttpPut("update")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDTO dto)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = _tokenService.GetUserIdFromToken(token);

                var user = await _userService.GetByIdAsync((ulong)userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Обновляем данные пользователя
                if (!string.IsNullOrEmpty(dto.Username))
                {
                    user.Username = dto.Username;
                }

                if (!string.IsNullOrEmpty(dto.Email))
                {
                    user.Email = dto.Email;
                }

                user.UpdatedAt = DateTime.UtcNow;

                // Загружаем аватар если предоставлен
                string? avatarUrl = null;
                if (dto.Avatar != null && dto.Avatar.Length > 0)
                {
                    // Проверка типа файла
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(dto.Avatar.ContentType.ToLower()))
                    {
                        return BadRequest("Only image files are allowed (JPEG, PNG, GIF)");
                    }

                    // Проверка размера (макс 5MB)
                    if (dto.Avatar.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("File size must not exceed 5MB");
                    }

                    using (var stream = dto.Avatar.OpenReadStream())
                    {
                        var photoFile = await _fileService.UploadUserAvatarAsync(userId, stream, dto.Avatar.FileName, dto.Avatar.ContentType, dto.Avatar.Length);
                        avatarUrl = _fileService.GetFileUrl(photoFile.Path);
                    }
                }

                await _userService.UpdateAsync(user);

                return Ok(new
                {
                    message = "User updated successfully",
                    username = user.Username,
                    email = user.Email,
                    avatarUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("profile/{id}")]
        public async Task<UserProfileDTO> GetUserProfile(ulong id)
        {
            return await _userService.GetUserProfileAsync(id);
        }

        // GET: api/User/profile - получение профиля по токену авторизации
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDTO>> GetCurrentUserProfile()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = _tokenService.GetUserIdFromToken(token);
                var profile = await _userService.GetUserProfileAsync(userId);

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/User/products - получение продуктов пользователя
        [HttpGet("products")]
        public async Task<ActionResult<ICollection<ProductDto>>> GetUserProducts()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = _tokenService.GetUserIdFromToken(token);
                var products = await _userService.GetUserProductsByIdAsync(userId);

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    RegionId = (int?)p.RegionId,
                    Region = p.Region,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Files = p.Files?.Select(f => new ProductFileDto
                    {
                        Id = f.Id,
                        Path = f.Path,
                        FileName = f.FileName,
                        FileType = f.FileType,
                        DisplayOrder = f.DisplayOrder,
                        MimeType = f.MimeType
                    }).ToList() ?? new List<ProductFileDto>(),
                    Licenses = p.Licenses ?? new List<License>()
                }).ToList();

                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

