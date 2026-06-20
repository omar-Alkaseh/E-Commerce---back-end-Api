using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByIdWithImagesAsync(int productId);
        Task<List<Product>> GetByCategoryIdAsync(int categoryId);
        Task<List<Product>> GetFeaturedAsync();
        Task<List<Product>> GetActiveProductsAsync();
        Task<List<Product>> GetTopRatedProductsAsync();
        Task<List<Product>> SearchAsync(string searchTerm);
        Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize);
        Task<List<Product>> GetAllWithMainImageAsync();
        Task<List<Product>> GetPagedWithMainImageAsync(int pageNumber, int pageSize);
        Task<List<Product>> GetProductsByMinimumRatingAsync(decimal minRating);

        Task<int> CountAsync();
        Task<bool> ExistsActiveProductAsync(int productId);
        Task<bool> SkuExistsAsync(string sku);
        Task<bool> SlugExistsAsync(string slug);
        Task<bool> SkuExistsForOtherProductAsync(string sku, int productId);
        Task<bool> SlugExistsForOtherProductAsync(string slug, int productId);
        Task<bool> HasStockAsync(int productId, int quantity);
    }
}
