using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Enums;
using Entities.Models;

namespace Application.Services
{
    public class BillService(IBillRepository repository, IUserService userService) : GenericService<Bill>(repository), IBillService
    {
        public async Task<Bill> CreateBillAsync(int userId, decimal totalAmount, BillStatus status)
        {
            var user = await userService.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' does not exist");
            }

            var bill = new Bill
            {
                User = user,
                TotalAmount = totalAmount,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.Now
            };

            await repository.AddAsync(bill);

            return bill;
        }

        public async Task<ICollection<Bill>> GetUserBillsByIdAsync(int id)
        {
            return repository.GetUserBillsByIdAsync(id);
        }
    }
}
