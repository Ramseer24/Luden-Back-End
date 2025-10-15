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
        public class UserController : ControllerBase
        {
            private readonly IUserService _userService;

            public UserController(IUserService userService)
            {
                _userService = userService;
            }

            // GET: api/User
            [HttpGet]
            public async Task<ActionResult> GetUsers()
            {
                //var users = await _userService.GetAllUsersAsync();
                return Ok();
            }

            // GET: api/User/5
            [HttpGet("{id}")]
            public async Task<ActionResult<User>> GetUser(int id)
            {
                try
                {
                    var user = await _userService.GetUserByIdAsync(id);
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

            // POST: api/User
            [HttpPost]
            public async Task<ActionResult<User>> PostUser([FromBody] UserCreateDto userDto)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                try
                {
                    var user = await _userService.CreateUserAsync(
                        userDto.Username,
                        userDto.Email,
                        userDto.Password,
                        userDto.Role
                    );
                    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception)
                {
                    return StatusCode(500, "An error occurred while creating the user.");
                }
            }

            // PUT: api/User/5
            [HttpPut("{id}")]
            public async Task<IActionResult> PutUser(int id, [FromBody] User user)
            {
                if (id != user.Id)
                {
                    return BadRequest("User ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                try
                {
                    var existingUser = await _userService.GetUserByIdAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    await _userService.UpdateUserAsync(user);
                    return NoContent();
                }
                catch (Exception)
                {
                    return StatusCode(500, "An error occurred while updating the user.");
                }
            }

            // DELETE: api/User/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteUser(int id)
            {
                try
                {
                    var user = await _userService.GetUserByIdAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    await _userService.DeleteUserAsync(id);
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
