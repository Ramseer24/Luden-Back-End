using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.ProductDTOs;
using Entities.Models;

namespace Application.Services
{
    public class ProductService : GenericService<Product>, IProductService
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IFileService _fileService;

        public ProductService(
            IGenericRepository<Product> productRepository,
            IFileService fileService) : base(productRepository)
        {
            _productRepository = productRepository;
            _fileService = fileService;
        }

        public async Task<ProductDto> GetProductDtoByIdAsync(ulong id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            return await MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductDtosAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var dtos = new List<ProductDto>();
            foreach (var product in products)
            {
                dtos.Add(await MapToDto(product));
            }
            return dtos;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, Stream? coverStream = null, string? coverFileName = null, string? coverContentType = null, long? coverFileSize = null)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                RegionId = dto.RegionId,
                DiscountPercentage = dto.DiscountPercentage,
                Developer = dto.Developer ?? string.Empty,
                Publisher = dto.Publisher ?? string.Empty,
                ReleaseDate = dto.ReleaseDate,
                Category = dto.Category ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);

            // Если передан файл обложки, загружаем его и устанавливаем как обложку
            if (coverStream != null && !string.IsNullOrEmpty(coverFileName) && !string.IsNullOrEmpty(coverContentType))
            {
                var fileSize = coverFileSize ?? 0;
                var coverFile = await _fileService.UploadImageAsync(
                    null,
                    product.Id,
                    coverStream,
                    coverFileName,
                    coverContentType,
                    fileSize);

                product.CoverFileId = coverFile.Id;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);
            }

            return await MapToDto(product);
        }

        public async Task<ProductDto> UpdateProductAsync(ulong id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            if (!string.IsNullOrEmpty(dto.Name))
            {
                product.Name = dto.Name;
            }

            if (!string.IsNullOrEmpty(dto.Description))
            {
                product.Description = dto.Description;
            }

            if (dto.Price.HasValue)
            {
                product.Price = dto.Price.Value;
            }

            if (dto.Stock.HasValue)
            {
                product.Stock = dto.Stock.Value;
            }

            if (dto.RegionId.HasValue)
            {
                product.RegionId = dto.RegionId.Value;
            }

            if (dto.DiscountPercentage.HasValue)
            {
                product.DiscountPercentage = dto.DiscountPercentage.Value;
            }

            if (dto.Developer != null) product.Developer = dto.Developer;
            if (dto.Publisher != null) product.Publisher = dto.Publisher;
            if (dto.ReleaseDate.HasValue) product.ReleaseDate = dto.ReleaseDate;
            if (dto.Category != null) product.Category = dto.Category;

            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            return await MapToDto(product);
        }

        public async Task<ProductDto> SetProductCoverAsync(ulong productId, ulong coverFileId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found");
            }

            // Проверяем, что файл существует и принадлежит этому продукту
            var coverFile = await _fileService.GetImageFileByIdAsync(coverFileId);
            if (coverFile == null)
            {
                throw new KeyNotFoundException($"File with ID {coverFileId} not found");
            }

            if (coverFile.ProductId != productId)
            {
                throw new InvalidOperationException($"File with ID {coverFileId} does not belong to product {productId}");
            }

            product.CoverFileId = coverFileId;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            return await GetProductDtoByIdAsync(productId);
        }

        private async Task<ProductDto> MapToDto(Product product)
        {
            var files = product.Files?.Select(f => new ProductFileDto
            {
                Id = f.Id,
                Path = f.Path,
                FileName = f.FileName,
                MimeType = f.MimeType,
                Width = f.Width,
                Height = f.Height,
                UserId = f.UserId,
                ProductId = f.ProductId,
                Url = _fileService.GetFileUrl(f.Path)
            }).ToList() ?? new List<ProductFileDto>();

            string? coverUrl = null;
            if (product.CoverFileId.HasValue)
            {
                var coverFile = await _fileService.GetImageFileByIdAsync(product.CoverFileId.Value);
                if (coverFile != null)
                {
                    coverUrl = _fileService.GetFileUrl(coverFile.Path);
                }
            }

            //// Если обложка отсутствует, используем плейсхолдер
            //if (string.IsNullOrEmpty(coverUrl))
            //{
            //    coverUrl = $"https://via.placeholder.com/600x400/cccccc/666666?text={Uri.EscapeDataString(product.Name)}";
            //}

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                RegionId = (int?)product.RegionId,
                DiscountPercentage = product.DiscountPercentage,
                SalesCount = product.SalesCount,
                Region = product.Region,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Developer = product.Developer,
                Publisher = product.Publisher,
                ReleaseDate = product.ReleaseDate,
                Category = product.Category,
                CoverUrl = coverUrl,
                Files = files,
                Licenses = product.Licenses ?? new List<License>()
            };
        }
    }
}

