using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ProductImageRepository : BaseRepository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(AppDbContext context) : base(context)
        {
        }


        public async Task ClearMainImageAsync(int productId)
        {
            var images = await _context.ProductImages
                .Where(i => i.ProductId == productId && i.IsMain)
                .ToListAsync();

            foreach (var image in images)
                image.IsMain = false;
        }

        public async Task<ProductImage?> GetByIdAndProductIdAsync(int productImageId, int productId) =>
            await _context.ProductImages
                .FirstOrDefaultAsync(i => i.ProductImageId == productImageId && i.ProductId == productId);

        public async Task<List<ProductImage>> GetByProductIdAsync(int productId) =>
            await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderByDescending(i => i.IsMain)
                .ThenBy(i => i.SortOrder)
                .ThenBy(i => i.ProductImageId)
                .ToListAsync();

        public async Task<int> GetNextSortOrderAsync(int productId)
        {
            var maxSortOrder = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .Select(i => (int?)i.SortOrder)
                .MaxAsync();

            return (maxSortOrder ?? 0) + 1;
        }

        public async Task<bool> ProductHasMainImageAsync(int productId) =>
            await _context.ProductImages
                .AnyAsync(i => i.ProductId == productId && i.IsMain);
    }
}
