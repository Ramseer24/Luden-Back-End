using Application.DTOs.UserDTOs;
using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IUserService : IGenericService<User>
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task UpdateUserAsync(UpdateUserDTO dto);
        Task<UserProfileDTO> GetuserProfileAsync();
    }
}
