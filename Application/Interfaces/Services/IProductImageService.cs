using Application.Features.ProductImages.DTOs;

namespace Application.Interfaces.Services
{
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(int productId);
        Task<ProductImageDto> AddAsync(AddProductImageDto request);
        Task<ProductImageDto> UpdateAsync(UpdateProductImageDto request);
        Task DeleteAsync(int productId, int productImageId);
        Task SetMainAsync(int productId, int productImageId);
    }
}