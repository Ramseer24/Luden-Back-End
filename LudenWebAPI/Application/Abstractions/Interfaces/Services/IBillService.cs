using Application.DTOs.BillDTOs;
using Entities.Enums;
using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IBillService : IGenericService<Bill>
    {
        Task<Bill> CreateBillAsync(ulong userId, decimal totalAmount, BillStatus status);
        Task<IEnumerable<Bill>> GetBillsByUserIdAsync(ulong userId);
        Task<BillDto> GetBillDtoByIdAsync(ulong id);
        Task<IEnumerable<BillDto>> GetAllBillDtosAsync();
        Task<IEnumerable<BillDto>> GetBillDtosByUserIdAsync(ulong userId);
    }
}
