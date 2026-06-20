using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context)
        {
            
        }



        private IQueryable<Review> ReviewsWithDetails()
        {
            return _context.Reviews
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Include(r => r.Product)
                    .ThenInclude(p => p.ProductImages);
        }


        public async Task<bool> ExistsByCustomerAndProductAsync(int customerId, int ProductId) =>
            await _context.Reviews
                .AnyAsync(r => r.CustomerId == customerId && r.ProductId == ProductId);

        public async Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId) =>
            await ReviewsWithDetails()
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();


        public async Task<IEnumerable<Review>> GetApprovedByProductIdAsync(int productId) =>
            await ReviewsWithDetails()
                .AsNoTracking()
                .Where(r => r.ProductId == productId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<Review?> GetByIdWithDetailsAsync(int reviewId) =>
            await ReviewsWithDetails()
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync() =>
            await ReviewsWithDetails()
                .AsNoTracking()
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
    }
}
