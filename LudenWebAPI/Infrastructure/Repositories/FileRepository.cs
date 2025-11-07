using Application.Abstractions.Interfaces.Repository;
using Entities.Models;
using Infrastructure.Extentions;
using Infrastructure.FirebaseDatabase;
using Microsoft.EntityFrameworkCore;
using FileEntity = Entities.Models.File;

namespace Infrastructure.Repositories
{
    public class FileRepository : GenericRepository<FileEntity>, IFileRepository
    {
        private readonly LudenDbContext? _context; //для старой БД
        private readonly bool _useFirebase;        //переключатель режима

        public FileRepository(LudenDbContext context) : base(null!)
        {
            _context = context;
            _useFirebase = false;
        }

        public FileRepository(FirebaseRepository firebaseRepo) : base(firebaseRepo)
        {
            _useFirebase = true;
        }

        // --- Методы работают в двух режимах ---

        public async Task<PhotoFile?> GetPhotoFileByIdAsync(int id)
        {
            if (_useFirebase)
            {
                var result = await GetByIdAsync((ulong)id);
                return result as PhotoFile;
            }

            return await _context!.Set<PhotoFile>()
                .Include(pf => pf.User)
                .FirstOrDefaultAsync(pf => pf.Id.ToInt() == id);
        }

        public async Task<ProductFile?> GetProductFileByIdAsync(int id)
        {
            if (_useFirebase)
            {
                var result = await GetByIdAsync((ulong)id);
                return result as ProductFile;
            }

            return await _context!.Set<ProductFile>()
                .Include(pf => pf.Product)
                .FirstOrDefaultAsync(pf => pf.Id.ToInt() == id);
        }

        public async Task<IEnumerable<ProductFile>> GetFilesByProductIdAsync(int productId)
        {
            if (_useFirebase)
            {
                var all = await GetAllAsync();
                return all
                    .OfType<ProductFile>()
                    .Where(pf => pf.ProductId == productId.ToUlong())
                    .OrderBy(pf => pf.DisplayOrder)
                    .ToList();
            }

            return await _context!.Set<ProductFile>()
                .Where(pf => pf.ProductId == productId.ToUlong())
                .OrderBy(pf => pf.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PhotoFile?> GetUserAvatarAsync(int userId)
        {
            if (_useFirebase)
            {
                var all = await GetAllAsync();
                return all.OfType<PhotoFile>().FirstOrDefault(pf => pf.UserId == userId.ToUlong());
            }

            return await _context!.Set<PhotoFile>()
                .FirstOrDefaultAsync(pf => pf.UserId == userId.ToUlong());
        }

        public async Task<PhotoFile> AddPhotoFileAsync(PhotoFile photoFile)
        {
            if (_useFirebase)
            {
                await AddAsync(photoFile);
                return photoFile;
            }

            await _context!.Set<PhotoFile>().AddAsync(photoFile);
            await _context.SaveChangesAsync();
            return photoFile;
        }

        public async Task<ProductFile> AddProductFileAsync(ProductFile productFile)
        {
            if (_useFirebase)
            {
                await AddAsync(productFile);
                return productFile;
            }

            await _context!.Set<ProductFile>().AddAsync(productFile);
            await _context.SaveChangesAsync();
            return productFile;
        }

        public async Task DeletePhotoFileAsync(int id)
        {
            if (_useFirebase)
            {
                await RemoveByIdAsync((ulong)id);
                return;
            }

            var photoFile = await GetPhotoFileByIdAsync(id);
            if (photoFile != null)
            {
                _context!.Set<PhotoFile>().Remove(photoFile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProductFileAsync(int id)
        {
            if (_useFirebase)
            {
                await RemoveByIdAsync((ulong)id);
                return;
            }

            var productFile = await GetProductFileByIdAsync(id);
            if (productFile != null)
            {
                _context!.Set<ProductFile>().Remove(productFile);
                await _context.SaveChangesAsync();
            }
        }
    }
}
