using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IBillRepository : IGenericRepository<Bill>
    {
        Task<IEnumerable<Bill>> GetBillsByUserIdAsync(int userId);
    }
}
