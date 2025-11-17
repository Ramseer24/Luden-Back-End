using Application.DTOs.BillDTOs;
using Application.DTOs.ProductDTOs;
using Entities.Enums;
using Entities.Models;

namespace Application.DTOs.UserDTOs
{
    public class UserProfileDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? AvatarUrl { get; set; }

        public ICollection<BillDto> Bills { get; set; }
        public ICollection<ProductDto> Products { get; set; }
    }
}
