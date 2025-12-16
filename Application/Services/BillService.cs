using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.BillDTOs;
using Application.DTOs.ProductDTOs;
using Entities.Enums;
using Entities.Models;

namespace Application.Services
{
    public class BillService(
        IBillRepository repository, 
        IUserService userService,
        IGenericRepository<BillItem> billItemRepository,
        IGenericRepository<Product> productRepository
        ) : GenericService<Bill>(repository), IBillService
    {
        public async Task<Bill> CreateBillAsync(ulong userId, decimal totalAmount, BillStatus status, string currency = "UAH", int bonusPointsUsed = 0, List<BillItemCreateDto>? items = null)
        {
            var user = await userService.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' does not exist");
            }

            if (bonusPointsUsed > 0)
            {
                if (user.BonusPoints < bonusPointsUsed)
                {
                    throw new InvalidOperationException("Insufficient bonus points");
                }
                user.BonusPoints -= bonusPointsUsed;
                await userService.UpdateAsync(user);
            }

            var bill = new Bill
            {
                UserId = user.Id,
                User = user,
                TotalAmount = totalAmount,
                Status = status,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.Now
            };

            await repository.AddAsync(bill);

            if (items != null)
            {
                foreach (var itemDto in items)
                {
                    var product = await productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product != null)
                    {
                        var billItem = new BillItem
                        {
                            BillId = bill.Id,
                            ProductId = itemDto.ProductId,
                            Quantity = itemDto.Quantity,
                            PriceAtPurchase = itemDto.Price,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Bill = bill,
                            Product = product
                        };
                        await billItemRepository.AddAsync(billItem);
                    }
                }
            }

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
            
            var billItemsRepo = billItemRepository;
            var allBillItems = await billItemsRepo.GetAllAsync();

            var productsRepo = productRepository;
            var allProducts = await productsRepo.GetAllAsync();

            foreach (var bill in bills)
            {
                bill.BillItems = allBillItems
                    .Where(bi => bi.BillId == bill.Id)
                    .ToList();

                foreach (var billItem in bill.BillItems)
                {
                    billItem.Product = allProducts.FirstOrDefault(p => p.Id == billItem.ProductId);
                }
            }
            
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
                    },
                    LicenseKeys = bi.Product.Licenses?
                        .Where(l => l.BillItemId == bi.Id)
                        .Select(l => l.LicenseKey)
                        .ToList() ?? new List<string>()
                }).ToList() ?? new List<BillItemDto>()
            };
        }
    }
}
