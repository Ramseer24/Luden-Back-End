﻿using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Services;
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
        public async Task<ActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
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
        public async Task<IActionResult> UpdateUser([FromForm] string? username, [FromForm] string? email, [FromForm] IFormFile? avatar)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = _tokenService.GetUserIdFromToken(token);

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Обновляем данные пользователя
                if (!string.IsNullOrEmpty(username))
                {
                    user.Username = username;
                }

                if (!string.IsNullOrEmpty(email))
                {
                    user.Email = email;
                }

                user.UpdatedAt = DateTime.UtcNow;

                // Загружаем аватар если предоставлен
                string? avatarUrl = null;
                if (avatar != null && avatar.Length > 0)
                {
                    // Проверка типа файла
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(avatar.ContentType.ToLower()))
                    {
                        return BadRequest("Only image files are allowed (JPEG, PNG, GIF)");
                    }

                    // Проверка размера (макс 5MB)
                    if (avatar.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("File size must not exceed 5MB");
                    }

                    using (var stream = avatar.OpenReadStream())
                    {
                        var photoFile = await _fileService.UploadUserAvatarAsync(userId, stream, avatar.FileName, avatar.ContentType, avatar.Length);
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
        public async Task<UserProfileDTO> GetUserProfile(int id)
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
        public async Task<ActionResult<ICollection<Product>>> GetUserProducts()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = _tokenService.GetUserIdFromToken(token);
                var products = await _userService.GetUserProductsByIdAsync(userId);

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

