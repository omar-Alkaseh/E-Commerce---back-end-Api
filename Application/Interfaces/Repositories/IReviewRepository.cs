using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetApprovedByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
        Task<Review?> GetByIdWithDetailsAsync(int reviewId);
        Task<bool> ExistsByCustomerAndProductAsync(int customerId, int ProductId);
    }
}
