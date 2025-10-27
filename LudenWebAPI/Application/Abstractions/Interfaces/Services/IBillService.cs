using Entities.Enums;
using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IBillService : IGenericService<Bill>
    {
        Task<Bill> CreateBillAsync(int userId, decimal totalAmount, BillStatus status);
        Task<IEnumerable<Bill>> GetBillsByUserIdAsync(int userId);
    }
}
