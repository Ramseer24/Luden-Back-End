using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Models;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class UserService(IUserRepository repository) : GenericService<User>(repository) , IUserService
    {
        public async Task<User> CreateUserAsync(string username, string email, string password, string role = "User")
        {
            if (await UsernameExistsAsync(username))
                throw new InvalidOperationException($"Username '{username}' already exists");

            if (await EmailExistsAsync(email))
                throw new InvalidOperationException($"Email '{email}' already exists");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow,
                Bills = new List<Bill>()
            };

            await repository.AddAsync(user);

            return user;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await repository.GetByIdAsync(id);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var users = await repository.GetAllAsync();
            return users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var users = await repository.GetAllAsync();
            return users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await repository.GetAllAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            await repository.RemoveByIdAsync(id);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var user = await GetUserByUsernameAsync(username);
            return user != null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            return user != null;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
