using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.UserDTOs;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudenWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController(ITokenService _tokenService, IAuthorizationService _authorizationService, IUserService userService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDTO registerData)
        {
            var result = await _authorizationService.RegisterAsync(registerData);

            if (result != RegisterStatus.Success)
            {
                return BadRequest(result.ToString());
            }
            return Ok(result.ToString());
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginData)
        {
            var result = await _authorizationService.LoginUserAsync(loginData);
            if (result != LoginStatus.Success)
            {
                return BadRequest(result.ToString());
            }
            //User? user;
            //if (!string.IsNullOrEmpty(loginData.googleJwtToken))
            //    user = await userService.GetByGoogleIdAsync(loginData.googleJwtToken);
            //else
            //    user = await userService.GetUserByEmailAsync(loginData.Email);
            var token = await _tokenService.GenerateToken(loginData);

            return Ok(new { token });
        }
    }
}
