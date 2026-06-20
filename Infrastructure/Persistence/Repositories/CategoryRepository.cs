using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository (AppDbContext context) : base(context)
        {
        
        }


        public async Task<Category?> GetByIdWithParentAsync(int categoryId)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<Category?> GetByIdWithChildrenAsync(int categoryId)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.InverseParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<IReadOnlyList<Category>> GetAllWithChildrenAsync()
        {
            return await _context.Categories
                .Include(c => c.InverseParentCategory)
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.CategoryId == categoryId);
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name == name);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Categories
                .AnyAsync(c => c.Slug == slug);
        }

        public async Task<bool> NameExistsForOtherCategoryAsync(int categoryId, string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.CategoryId != categoryId && c.Name == name);
        }

        public async Task<bool> SlugExistsForOtherCategoryAsync(int categoryId, string slug)
        {
            return await _context.Categories
                .AnyAsync(c => c.CategoryId != categoryId && c.Slug == slug);
        }
    }
}
