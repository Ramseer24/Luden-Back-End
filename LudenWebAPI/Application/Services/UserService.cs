using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.BillDTOs;
using Application.DTOs.ProductDTOs;
using Application.DTOs.UserDTOs;
using Entities.Models;

namespace Application.Services
{
    public class UserService(IUserRepository repository, IPasswordHasher passwordHasher, IFileRepository fileRepository, IFileService fileService) : GenericService<User>(repository), IUserService
    {
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var users = await repository.GetAllAsync();
            return users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var users = await repository.GetAllAsync();
            return users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await repository.GetAllAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var user = await GetUserByUsernameAsync(username);
            return user != null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            return user != null;
        }

        public async Task<ICollection<Bill>> GetUserBillsByIdAsync(ulong id)
        {
            return await repository.GetUserBillsByIdAsync(id);
        }

        public async Task<ICollection<Product>> GetUserProductsByIdAsync(ulong userId)
        {
            return await repository.GetUserProductsByIdAsync(userId);
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(ulong id)
        {
            User? user = await repository.GetByIdAsync(id);
            ICollection<Bill> bills = await GetUserBillsByIdAsync(id);
                ICollection<Product> products = await GetUserProductsByIdAsync(id);

            string? avatarUrl = null;
            if (user.AvatarFileId.HasValue)
            {
                var avatarFile = await fileRepository.GetImageFileByIdAsync(user.AvatarFileId.Value);
                if (avatarFile != null)
                {
                    avatarUrl = fileService.GetFileUrl(avatarFile.Path);
                }
            }

            UserProfileDTO dto = new()
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                BonusPoints = user.BonusPoints,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                AvatarUrl = avatarUrl,

                Bills = bills?.Select(b => new BillDto
                {
                    Id = b.Id,
                    CreatedAt = b.CreatedAt,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status.ToString(),
                    BillItems = b.BillItems?.Select(bi => new BillItemDto
                    {
                        Id = bi.Id,
                        Quantity = bi.Quantity,
                        Price = bi.PriceAtPurchase,
                        Product = new ProductDto
                        {
                            Id = bi.Product.Id,
                            Name = bi.Product.Name,
                            Description = bi.Product.Description,
                            Price = bi.Product.Price
                        }
                    }).ToList() ?? new List<BillItemDto>()
                }).ToList() ?? new List<BillDto>(),

                Products = products?.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Files = p.Files?.Select(f => new ProductFileDto
                    {
                        Id = f.Id,
                        Path = f.Path,
                        FileName = f.FileName,
                        MimeType = f.MimeType,
                        Width = f.Width,
                        Height = f.Height,
                        UserId = f.UserId,
                        ProductId = f.ProductId,
                        Url = fileService.GetFileUrl(f.Path)
                    }).ToList() ?? new List<ProductFileDto>()
                }).ToList() ?? new List<ProductDto>()
            };
            return dto;
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await repository.GetByGoogleIdAsync(googleId);
        }
    }
}
