using Application.Features.Reviews.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllByProductIdAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync();
        Task<ReviewDto> GetByReviewIdWithProductDetailsAsync(int reviewId);
        Task<ReviewDto> AdminApproveReviewAsync(int reviewId);
        Task<ReviewDto> CreateReviewAsync(int productId, CreateReviewDto request);
        Task<ReviewDto> PatchReviewAsync(int reviewId, PatchReviewDto request);
        Task DeleteAsync(int reviewId);
    
    }
}
