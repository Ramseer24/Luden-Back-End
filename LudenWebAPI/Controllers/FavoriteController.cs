using Application.Abstractions.Interfaces.Services;
using Application.DTOs.FavoriteDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LudenWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoriteController(IFavoriteService favoriteService) : ControllerBase
    {
        /// <summary>
        /// Получить все избранные продукты текущего пользователя
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteDto>>> GetFavorites()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                if (!ulong.TryParse(userIdClaim.Value, out ulong userId))
                {
                    return BadRequest("Invalid user ID format");
                }

                var favorites = await favoriteService.GetUserFavoritesAsync(userId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving favorites: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверить, находится ли продукт в избранном
        /// </summary>
        [HttpGet("check/{productId}")]
        public async Task<ActionResult<bool>> IsFavorite(ulong productId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                if (!ulong.TryParse(userIdClaim.Value, out ulong userId))
                {
                    return BadRequest("Invalid user ID format");
                }

                var isFavorite = await favoriteService.IsFavoriteAsync(userId, productId);
                return Ok(new { isFavorite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while checking favorite: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавить продукт в избранное
        /// </summary>
        [HttpPost("{productId}")]
        public async Task<ActionResult<FavoriteDto>> AddFavorite(ulong productId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                if (!ulong.TryParse(userIdClaim.Value, out ulong userId))
                {
                    return BadRequest("Invalid user ID format");
                }

                var favorite = await favoriteService.AddFavoriteAsync(userId, productId);
                return CreatedAtAction(nameof(GetFavorites), new { }, favorite);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding favorite: {ex.Message}");
            }
        }

        /// <summary>
        /// Удалить продукт из избранного
        /// </summary>
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFavorite(ulong productId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token");
                }

                if (!ulong.TryParse(userIdClaim.Value, out ulong userId))
                {
                    return BadRequest("Invalid user ID format");
                }

                await favoriteService.RemoveFavoriteAsync(userId, productId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Favorite not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while removing favorite: {ex.Message}");
            }
        }
    }
}

