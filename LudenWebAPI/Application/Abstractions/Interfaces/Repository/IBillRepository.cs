using Entities.Models;

namespace Application.Abstractions.Interfaces.Repository
{
    public interface IBillRepository : IGenericRepository<Bill>
    {
        // Add any Bill-specific repository methods here if needed
        ICollection<Bill> GetUserBillsByIdAsync(int id);
    }
}
