using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.DTOs.UserDTOs;
using Entities.Config;
using Entities.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Application.Services
{

    public class BaseTokenService(Config config, IUserRepository userRepository, IGoogleTokenValidator googleTokenValidator) : ITokenService
    {
        private readonly string _secretKey = config.Jwt.Key;
        private readonly string _issuer = config.Jwt.Issuer;
        private readonly string _audience = config.Jwt.Audience;
        private readonly int _tokenExpiresHours = config.Jwt.TokenExpiresHours;


        public async Task<string?> GenerateToken(UserLoginDTO loginData)
        {
            User? user = null;

            if (!string.IsNullOrEmpty(loginData.Email))
            {
                user = await userRepository.GetByEmailAsync(loginData.Email);
            }
            else if (!string.IsNullOrEmpty(loginData.googleJwtToken))
            {
                var googleToken = await googleTokenValidator.ValidateAsync(loginData.googleJwtToken);
                user = await userRepository.GetByGoogleIdAsync(googleToken.Subject);
            }

            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return GenerateJwtToken(user);
        }

        public string GenerateJwtToken(User user)
        {
            Claim[] claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ];

            if (!string.IsNullOrEmpty(user.Username))
            {
                claims = [.. claims, new Claim(ClaimTypes.Name, user.Username)];
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims = [.. claims, new Claim(ClaimTypes.Email, user.Email)];
            }

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_secretKey));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_tokenExpiresHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ulong GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "Id");

            if (userIdClaim == null || !ulong.TryParse(userIdClaim.Value, out ulong userId))
            {
                throw new SecurityTokenException("Id claim missing or invalid");
            }

            return userId;
        }

    }
}
