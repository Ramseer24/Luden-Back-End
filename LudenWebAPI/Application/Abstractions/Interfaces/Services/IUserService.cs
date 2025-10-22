using Application.DTOs.UserDTOs;
using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IUserService : IGenericService<User>
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task UpdateUserAsync(UpdateUserDTO dto);
        Task<UserProfileDTO> GetUserProfileAsync(int id);
        Task<ICollection<Bill>> GetUserBillsByIdAsync(int id);
    }
}
