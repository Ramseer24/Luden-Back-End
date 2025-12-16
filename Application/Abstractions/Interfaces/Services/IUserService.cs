using Application.DTOs.UserDTOs;
using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IUserService : IGenericService<User>
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<UserProfileDTO> GetUserProfileAsync(ulong id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<ICollection<Bill>> GetUserBillsByIdAsync(ulong id);
        Task<ICollection<Product>> GetUserProductsByIdAsync(ulong userId);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}
