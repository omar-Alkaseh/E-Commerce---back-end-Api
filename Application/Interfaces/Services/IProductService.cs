using Application.Common.Models;
using Application.Features.Products.DTOs;

namespace Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();

        Task<PaginatedResult<ProductDto>> GetPagedAsync(int pageNumber, int pageSize);

        Task<List<ProductDto>> GetProductsByMinimumRatingAsync(int minRating);
        Task<List<ProductDto>> GetTopRatedProductsAsync();

        Task<ProductDto?> GetByIdAsync(int productId);
        Task<ProductDto?> GetByIdWithImagesAsync(int productId);

        Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetFeaturedAsync();
        Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
        Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm);

        Task<ProductDto> PatchAsync(PatchProductDto request);
        Task<ProductDto> CreateAsync(CreateProductDto request);
        Task<ProductDto> UpdateAsync(UpdateProductDto request);
        Task DeleteAsync(int productId);
    }
}
