using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByGoogleIdAsync(string googleId);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> IsPasswordValidByEmailAsync(string email, string passwordHash);
    }
}
