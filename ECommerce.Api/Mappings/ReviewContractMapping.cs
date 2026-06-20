using Application.Features.Reviews.DTOs;
using Contracts.Responses;

namespace ECommerce.Api.Mappings
{
    public static class ReviewContractMapping
    {
        public static ReviewResponse ToResponse(this ReviewDto review)
        {
            return new ReviewResponse
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                CustomerId = review.CustomerId,
                CustomerName = review.CustomerName,
                Rating = review.Rating,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,

                product = review.product is null ? null : new ReviewProductResponse
                {
                    ProductId = review.product.ProductId,
                    ProductName = review.product.ProductName,
                    MainImageUrl = review.product.MainImageUrl,
                    Slug = review.product.Slug
                }
            };
        }

        public static List<ReviewResponse> ToResponseList(this IEnumerable<ReviewDto> reviews) =>
            reviews.Select(r => r.ToResponse()).ToList();
    }
}
