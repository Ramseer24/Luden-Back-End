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
    public class BillService(IBillRepository billRepository, IGenericRepository<User> userRepository) : IBillService
    {
       
        public async Task<Bill> CreateBillAsync(int userId, decimal totalAmount, string status = "pending")
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' does not exist");
            }

            var bill = new Bill
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            await billRepository.AddAsync(bill);
            await billRepository.SaveChangesAsync();

            return bill;
        }

        public async Task<Bill> GetByIdAsync(int id)
        {
            return await billRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Bill>> GetAllAsync()
        {
            return await billRepository.GetAllAsync();
        }

        public async Task<Bill> CreateAsync(Bill entity)
        {
            await billRepository.AddAsync(entity);
            await billRepository.SaveChangesAsync();
            return entity;
        }

        public async Task<Bill> UpdateAsync(Bill entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            await billRepository.UpdateAsync(entity);
            await billRepository.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await billRepository.RemoveByIdAsync(id);
            await billRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Bill entity)
        {
            await billRepository.RemoveAsync(entity);
            await billRepository.SaveChangesAsync();
        }
    }
}
