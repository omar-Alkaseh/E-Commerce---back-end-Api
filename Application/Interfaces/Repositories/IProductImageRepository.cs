using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        Task<List<ProductImage>> GetByProductIdAsync(int productId);
        Task<ProductImage?> GetByIdAndProductIdAsync(int productImageId, int productId);
        Task<bool> ProductHasMainImageAsync(int productId);
        Task ClearMainImageAsync(int productId);
        Task<int> GetNextSortOrderAsync(int productId);
    }
}