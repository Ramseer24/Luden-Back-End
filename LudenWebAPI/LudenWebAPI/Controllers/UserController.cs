namespace LudenWebAPI.Controllers
{
    using Application.Abstractions.Interfaces.Services;
    using Entities.Models;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    namespace Sonar.Controllers.UserCore
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
                return Ok();
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
        }

        public class UserCreateDto
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Role { get; set; } = "User";
        }
    }
}
