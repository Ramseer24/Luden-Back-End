using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.BillDTOs;
using Application.DTOs.ProductDTOs;
using Entities.Enums;
using Entities.Models;

namespace Application.Services
{
    public class BillService(IBillRepository repository, IUserService userService) : GenericService<Bill>(repository), IBillService
    {
        public async Task<Bill> CreateBillAsync(ulong userId, decimal totalAmount, BillStatus status)
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

        public async Task<IEnumerable<Bill>> GetBillsByUserIdAsync(ulong userId)
        {
            return await repository.GetBillsByUserIdAsync(userId);
        }

        public async Task<BillDto> GetBillDtoByIdAsync(ulong id)
        {
            var bill = await repository.GetByIdAsync(id);
            if (bill == null)
            {
                throw new KeyNotFoundException($"Bill with ID {id} not found");
            }

            return MapToDto(bill);
        }

        public async Task<IEnumerable<BillDto>> GetAllBillDtosAsync()
        {
            var bills = await repository.GetAllAsync();
            return bills.Select(MapToDto);
        }

        public async Task<IEnumerable<BillDto>> GetBillDtosByUserIdAsync(ulong userId)
        {
            var bills = await repository.GetBillsByUserIdAsync(userId);
            return bills.Select(MapToDto);
        }

        private BillDto MapToDto(Bill bill)
        {
            return new BillDto
            {
                Id = bill.Id,
                CreatedAt = bill.CreatedAt,
                TotalAmount = bill.TotalAmount,
                Status = bill.Status.ToString(),
                BillItems = bill.BillItems?.Select(bi => new BillItemDto
                {
                    Id = bi.Id,
                    Quantity = bi.Quantity,
                    Price = bi.PriceAtPurchase,
                    Product = new ProductDto
                    {
                        Id = bi.Product.Id,
                        Name = bi.Product.Name,
                        Description = bi.Product.Description,
                        Price = bi.Product.Price,
                        Stock = bi.Product.Stock,
                        RegionId = (int?)bi.Product.RegionId,
                        Region = bi.Product.Region,
                        CreatedAt = bi.Product.CreatedAt,
                        UpdatedAt = bi.Product.UpdatedAt
                    }
                }).ToList() ?? new List<BillItemDto>()
            };
        }
    }
}
