using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.FirebaseDatabase;
using FileEntity = Entities.Models.File;

namespace Infrastructure.Repositories
{
    public class FileRepository : GenericRepository<FileEntity>, IFileRepository
    {
        public FileRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
        }

        public async Task<PhotoFile?> GetPhotoFileByIdAsync(ulong id)
        {
            var result = await GetByIdAsync(id);
            return result as PhotoFile;
        }

        public async Task<ProductFile?> GetProductFileByIdAsync(ulong id)
        {
            var result = await GetByIdAsync(id);
            return result as ProductFile;
        }

        public async Task<IEnumerable<ProductFile>> GetFilesByProductIdAsync(ulong productId)
        {
            var all = await GetAllAsync();
            return all
                .OfType<ProductFile>()
                .Where(pf => pf.ProductId == productId)
                .OrderBy(pf => pf.DisplayOrder)
                .ToList();
        }

        public async Task<PhotoFile?> GetUserAvatarAsync(ulong userId)
        {
            var all = await GetAllAsync();
            return all.OfType<PhotoFile>().FirstOrDefault(pf => pf.UserId == userId);
        }

        public async Task<PhotoFile> AddPhotoFileAsync(PhotoFile photoFile)
        {
            await AddAsync(photoFile);
            return photoFile;
        }

        public async Task<ProductFile> AddProductFileAsync(ProductFile productFile)
        {
            await AddAsync(productFile);
            return productFile;
        }

        public async Task DeletePhotoFileAsync(ulong id)
        {
            await RemoveByIdAsync(id);
        }

        public async Task DeleteProductFileAsync(ulong id)
        {
            await RemoveByIdAsync(id);
        }
    }
}
