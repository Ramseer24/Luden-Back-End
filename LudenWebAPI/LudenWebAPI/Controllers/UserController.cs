using Application.Abstractions.Interfaces.Services;
using Application.DTOs.UserDTOs;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
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
                var user = await userService.GetUserByIdAsync(id);
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
                var user = await userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                await userService.DeleteUserAsync(id);
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
            return Ok(userService.UpdateUserAsync(updateUserDTO));
        }
        //[HttpGet("profile/{id}")]
        //public async Task<UserProfileDTO> GetUserProfile()
        //{

        //}
    }
}

