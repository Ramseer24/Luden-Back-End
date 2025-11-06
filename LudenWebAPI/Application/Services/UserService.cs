using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.BillDTOs;
using Application.DTOs.ProductDTOs;
using Application.DTOs.UserDTOs;
using Entities.Models;

namespace Application.Services
{
    public class UserService(IUserRepository repository, IPasswordHasher passwordHasher, IFileRepository fileRepository) : GenericService<User>(repository), IUserService
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

        public async Task UpdateUserAsync(UpdateUserDTO dto)
        {
            User user = await repository.GetByIdAsync(dto.Id);
            user.Username = dto.Username;
            user.Email = dto.Email;
            user.PasswordHash = passwordHasher.Hash(dto.Password);
            user.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(user);
        }
        public async Task<ICollection<Bill>> GetUserBillsByIdAsync(int id)
        {
            return await repository.GetUserBillsByIdAsync(id);
        }

        public async Task<ICollection<Product>> GetUserProductsByIdAsync(int userId)
        {
            return await repository.GetUserProductsByIdAsync(userId);
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(int id)
        {
            User? user = await repository.GetByIdAsync((ulong)id);
            ICollection<Bill> bills = await GetUserBillsByIdAsync(id);
            ICollection<Product> products = await GetUserProductsByIdAsync(id);

            string? avatarUrl = null;
            if (user.AvatarFileId.HasValue)
            {
                var avatarFile = await fileRepository.GetPhotoFileByIdAsync((int)user.AvatarFileId.Value);
                if (avatarFile != null)
                {
                    avatarUrl = $"/uploads/{avatarFile.Path.Replace("\\", "/")}";
                }
            }

            UserProfileDTO dto = new()
            {
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                AvatarUrl = avatarUrl,

                Bills = bills.Select(b => new BillDto
                {
                    Id = (int)b.Id,
                    CreatedAt = b.CreatedAt,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status.ToString(),
                    BillItems = b.BillItems.Select(bi => new BillItemDto
                    {
                        Id = (int)bi.Id,
                        Quantity = bi.Quantity,
                        Price = bi.PriceAtPurchase,
                        Product = new ProductDto
                        {
                            Id = (int)bi.Product.Id,
                            Name = bi.Product.Name,
                            Description = bi.Product.Description,
                            Price = bi.Product.Price
                        }
                    }).ToList()
                }).ToList(),

                Products = products.Select(p => new ProductDto
                {
                    Id = (int)p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Files = p.Files.Select(f => new ProductFileDto
                    {
                        Id = (int)f.Id,
                        Path = f.Path,
                        FileName = f.FileName,
                        FileType = f.FileType,
                        DisplayOrder = f.DisplayOrder,
                        MimeType = f.MimeType
                    }).ToList()
                }).ToList()
            };
            return dto;
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await repository.GetByGoogleIdAsync(googleId);
        }
    }
}
