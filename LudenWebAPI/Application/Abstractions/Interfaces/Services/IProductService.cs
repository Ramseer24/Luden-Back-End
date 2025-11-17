using Application.DTOs.ProductDTOs;
using Entities.Models;

namespace Application.Abstractions.Interfaces.Services
{
    public interface IProductService : IGenericService<Product>
    {
        Task<ProductDto> GetProductDtoByIdAsync(ulong id);
        Task<IEnumerable<ProductDto>> GetAllProductDtosAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto dto, Stream? coverStream = null, string? coverFileName = null, string? coverContentType = null, long? coverFileSize = null);
        Task<ProductDto> UpdateProductAsync(ulong id, UpdateProductDto dto);
        Task<ProductDto> SetProductCoverAsync(ulong productId, ulong coverFileId);
    }
}

