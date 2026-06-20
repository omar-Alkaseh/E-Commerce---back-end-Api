using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) 
            : base(context)
        {
        }

        public async Task<Product?> GetByIdWithImagesAsync(int productId) =>
             await _context.Products
                .Include(p => p.ProductImages.OrderByDescending(i => i.IsMain))
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);

        public async Task<List<Product>> GetByCategoryIdAsync(int categoryId) =>
             await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync();

        public async Task<bool> ExistsActiveProductAsync(int productId) =>
             await _context.Products
                .AnyAsync(p => p.ProductId == productId && p.IsActive);

        public async Task<List<Product>> GetFeaturedAsync() =>
            await _context.Products
                .AsNoTracking()
                .Where(p => p.IsFeatured && p.IsActive)
                .ToListAsync();

        public async Task<List<Product>> GetActiveProductsAsync() =>
            await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();

        public async Task<List<Product>> SearchAsync(string searchTerm) 
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Product>();

            searchTerm = searchTerm.Trim();

            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Reviews)
                .Where(p =>
                    p.IsActive &&
                    p.Name.Contains(searchTerm) ||
                    p.Sku.Contains(searchTerm) ||
                    (p.Description != null && p.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<bool> SkuExistsAsync(string sku) =>
            await _context.Products
                .AnyAsync(p => p.Sku == sku);

        public async Task<bool> SkuExistsForOtherProductAsync(string sku, int productId) =>
            await _context.Products
                .AnyAsync(p => p.Sku == sku && p.ProductId != productId);

        public async Task<bool> HasStockAsync(int productId, int quantity) =>
            await _context.Products
                .AnyAsync(p => p.ProductId == productId && p.StockQuantity >= quantity);

        public async Task<bool> SlugExistsAsync(string slug) =>
            await _context.Products
                .AnyAsync(p => p.Slug == slug);

        public async Task<bool> SlugExistsForOtherProductAsync(string slug, int productId) =>
            await _context.Products
                .AnyAsync(p => p.Slug == slug && p.ProductId != productId);

        public async Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize) =>
            await _context.Products
                .AsNoTracking()
                .Include(p => p.Reviews)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        public async Task<int> CountAsync() =>
            await _context.Products.CountAsync(p => p.IsActive);

        public async Task<List<Product>> GetAllWithMainImageAsync() =>
            await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.ProductImages
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.SortOrder))
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<List<Product>> GetPagedWithMainImageAsync(int pageNumber, int pageSize) =>
            await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.ProductImages
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.SortOrder))
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<List<Product>> GetTopRatedProductsAsync() =>
             await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.ProductImages
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.SortOrder))
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .OrderByDescending(p => 
                    p.Reviews
                        .Where(r => r.IsApproved)
                        .Average(r => (decimal?) r.Rating) ?? 0)
                .ThenByDescending(p => 
                    p.Reviews.Count(r => r.IsApproved))
                .ToListAsync();



        public async Task<List<Product>> GetProductsByMinimumRatingAsync(decimal minRating) =>
            await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Where(p =>
                    (p.Reviews
                        .Where(r => r.IsApproved)
                        .Average(r => (decimal?)r.Rating) ?? 0) >= minRating)
                .Include(p => p.ProductImages
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.SortOrder))
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .OrderByDescending(p =>
                    p.Reviews
                        .Where(r => r.IsApproved)
                        .Average(r => (decimal?)r.Rating) ?? 0)
                .ToListAsync();
    }
}
