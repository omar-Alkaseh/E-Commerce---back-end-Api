using Application.Features.Reviews.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public static class ReviewMappingExtensions
    {
        public static ReviewDto ToDto(this Review review)
        {
            var fullName = review.Customer?.User is null
                ? "Unknown Customer"
                : $"{review.Customer.User.FirstName} {review.Customer.User.LastName}";

            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                CustomerId = review.CustomerId,
                Comment = review.Comment,
                CustomerName = fullName,
                Rating = review.Rating,
                IsApproved = review.IsApproved,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,

                product = review.Product is null ? null : new ReviewProductDto
                {
                    ProductId = review.ProductId,
                    ProductName = review.Product.Name,
                    Slug = review.Product.Slug,
                    MainImageUrl = review.Product.ProductImages.Where(pi => pi.IsMain).Select(pi => pi.ImageUrl).FirstOrDefault()
                }
            };
        }

        public static List<ReviewDto> ToDtoList(this IEnumerable<Review> reviews) =>
            reviews.Select(r => r.ToDto()).ToList();
    }
}
