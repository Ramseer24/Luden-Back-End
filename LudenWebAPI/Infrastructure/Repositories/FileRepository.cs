using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Extentions;
using Infrastructure.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using FileEntity = Entities.Models.File;

namespace Infrastructure.Repositories
{
    public class FileRepository : GenericRepository<FileEntity>, IFileRepository
    {
        public FileRepository(LudenDbContext context) : base(context)
        {
        }

        public async Task<PhotoFile?> GetPhotoFileByIdAsync(int id)
        {
            return await context.Set<PhotoFile>()
                .Include(pf => pf.User)
                .FirstOrDefaultAsync(pf => pf.Id.ToInt() == id);
        }

        public async Task<ProductFile?> GetProductFileByIdAsync(int id)
        {
            return await context.Set<ProductFile>()
                .Include(pf => pf.Product)
                .FirstOrDefaultAsync(pf => pf.Id.ToInt() == id);
        }

        public async Task<IEnumerable<ProductFile>> GetFilesByProductIdAsync(int productId)
        {
            return await context.Set<ProductFile>()
                .Where(pf => pf.ProductId == productId.ToUlong())
                .OrderBy(pf => pf.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PhotoFile?> GetUserAvatarAsync(int userId)
        {
            return await context.Set<PhotoFile>()
                .FirstOrDefaultAsync(pf => pf.UserId == userId.ToUlong());
        }

        public async Task<PhotoFile> AddPhotoFileAsync(PhotoFile photoFile)
        {
            await context.Set<PhotoFile>().AddAsync(photoFile);
            await context.SaveChangesAsync();
            return photoFile;
        }

        public async Task<ProductFile> AddProductFileAsync(ProductFile productFile)
        {
            await context.Set<ProductFile>().AddAsync(productFile);
            await context.SaveChangesAsync();
            return productFile;
        }

        public async Task DeletePhotoFileAsync(int id)
        {
            var photoFile = await GetPhotoFileByIdAsync(id);
            if (photoFile != null)
            {
                context.Set<PhotoFile>().Remove(photoFile);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductFileAsync(int id)
        {
            var productFile = await GetProductFileByIdAsync(id);
            if (productFile != null)
            {
                context.Set<ProductFile>().Remove(productFile);
                await context.SaveChangesAsync();
            }
        }
    }
}
