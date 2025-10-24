using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.UserDTOs;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserService userService, ITokenService tokenService) : ControllerBase
    {
        // GET: api/User
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await userService.GetByIdAsync(id);
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
                var user = await userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                await userService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        [HttpPatch("update")]
        public async Task<IActionResult> UdpateUserAsync([FromBody] UpdateUserDTO updateUserDTO)
        {
            await userService.UpdateUserAsync(updateUserDTO);
            return Ok();
        }
        [HttpGet("profile/{id}")]
        public async Task<UserProfileDTO> GetUserProfile(int id)
        {
            return await userService.GetUserProfileAsync(id);
        }

        // GET: api/User/profile - получение профиля по токену авторизации
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDTO>> GetCurrentUserProfile()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = tokenService.GetUserIdFromToken(token);
                var profile = await userService.GetUserProfileAsync(userId);

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

