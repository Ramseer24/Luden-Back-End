using Microsoft.AspNetCore.Http;

namespace Application.DTOs.UserDTOs
{
    public class UpdateUserDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
