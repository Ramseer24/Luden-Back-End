using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<bool> IsPasswordValidByEmailAsync(string email, string passwordHash); 
    }
}
