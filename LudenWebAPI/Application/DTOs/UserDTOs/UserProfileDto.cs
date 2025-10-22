using Entities.Models;

namespace Application.DTOs.UserDTOs
{
    public class UserProfileDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public ICollection<Bill> Bills { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
