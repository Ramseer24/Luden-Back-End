using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<bool> IsPasswordValidByEmailAsync(string email, string passwordHash);
        Task<ICollection<Bill>> GetUserBillsByIdAsync(int id);
    }
}
