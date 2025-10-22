using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.UserDTOs;
using Entities.Models;

namespace Application.Services
{
    public class UserService(IUserRepository repository, IPasswordHasher passwordHasher) : GenericService<User>(repository), IUserService
    {
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

        public async Task UpdateUserAsync(UpdateUserDTO dto)
        {
            User user = await repository.GetByIdAsync(dto.Id);
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.PasswordHash = passwordHasher.Hash(dto.Password);
            user.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(user);
        }
        public async Task<ICollection<Bill>> GetUserBillsByIdAsync(int id)
        {
            return await repository.GetUserBillsByIdAsync(id);
        }
        public async Task<UserProfileDTO> GetUserProfileAsync(int id)
        {
            User? user = await repository.GetByIdAsync(id);
            ICollection<Bill> bills = await GetUserBillsByIdAsync(id);
            UserProfileDTO dto = new()
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Bills = bills
            };
            return dto;
        }
    }
}
