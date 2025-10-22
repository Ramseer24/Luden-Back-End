using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.UserDTOs;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProfileService(IUserRepository userRepository, IBillRepository billRepository) : IProfileService
    {

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await userRepository.GetUserWithBillsAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            return new UserProfileDto
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Bills = user.Bills.Select(b => new BillDto
                {
                    TotalAmount = b.TotalAmount,
                    Status = b.Status
                }).ToList()
            };
        }

        public async Task SetUserProfileAsync(int id, UserProfileDto dto)
        {
            var user = await userRepository.GetUserWithBillsAsync(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Update user fields (excluding password for profile context)
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.Role = dto.Role;

            // Sync bills
            var billsToRemove = user.Bills.Where(b => !dto.Bills.Any(d => id == b.Id)).ToList();
            foreach (var bill in billsToRemove)
            {
                await billRepository.RemoveAsync(bill);
            }

            foreach (var billDto in dto.Bills)
            {                var bill = await billRepository.GetByIdAsync(id);
                if (bill != null && bill.UserId == id)
                {
                    bill.TotalAmount = billDto.TotalAmount;
                    bill.Status = billDto.Status;
                    await billRepository.UpdateAsync(bill);
                }
            }

            await userRepository.UpdateAsync(user);
        }
    }
}