using Entities.Enums;

namespace Application.DTOs.UserDTOs
{
    public class UserDto
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

