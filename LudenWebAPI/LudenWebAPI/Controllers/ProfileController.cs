using Application.Abstractions.Interfaces.Services;
using Application.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController(IProfileService profileService) : ControllerBase
    {
        // GET: api/Profile/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(int id)
        {
            try
            {
                var profile = await profileService.GetUserProfileAsync(id);
                if (profile == null)
                {
                    return NotFound();
                }
                return Ok(profile);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the profile.");
            }
        }

        // PUT: api/Profile/5
        [HttpPut("{id}")]
        public async Task<IActionResult> SetUserProfile(int id, [FromBody] UserProfileDto profileDto)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await profileService.SetUserProfileAsync(id, profileDto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the profile.");
            }
        }
    }
}
