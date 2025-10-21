using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BillService(IBillRepository repository,IUserService userService) : GenericService<Bill>(repository), IBillService
    {
       
        public async Task<Bill> CreateBillAsync(int userId, decimal totalAmount, string status)
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
                Status = string.IsNullOrEmpty(status) ? "pending" : status,
                CreatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(bill);

            return bill;
        }

        Task IGenericService<Bill>.CreateAsync(Bill entity)
        {
            return CreateAsync(entity);
        }
    }
}
