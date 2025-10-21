using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IUserService : IGenericService<User>
    {
       //Task<User> CreateUserAsync(string username, string email, string password, string role = "User");
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}
