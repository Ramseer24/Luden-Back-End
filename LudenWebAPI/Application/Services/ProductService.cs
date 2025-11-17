using Application.Abstractions.Interfaces.Repository;
using Application.Abstractions.Interfaces.Services;
using Application.DTOs.ProductDTOs;
using Entities.Models;

namespace Application.Services
{
    public class ProductService : GenericService<Product>, IProductService
    {
        private readonly IGenericRepository<Product> _productRepository;

        public ProductService(
            IGenericRepository<Product> productRepository) : base(productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto> GetProductDtoByIdAsync(ulong id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductDtosAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                RegionId = dto.RegionId,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            return MapToDto(product);
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

            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            return MapToDto(product);
        }

        private ProductDto MapToDto(Product product)
        {
            var files = product.Files?.Select(f => new ProductFileDto
            {
                Id = f.Id,
                Path = f.Path,
                FileName = f.FileName,
                FileType = f.FileType,
                DisplayOrder = f.DisplayOrder,
                MimeType = f.MimeType
            }).ToList() ?? new List<ProductFileDto>();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                RegionId = (int?)product.RegionId,
                Region = product.Region,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Files = files,
                Licenses = product.Licenses ?? new List<License>()
            };
        }
    }
}

