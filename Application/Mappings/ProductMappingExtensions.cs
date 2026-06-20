using Application.Features.ProductImages.DTOs;
using Application.Features.Products.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public static class ProductMappingExtensions
    {
        public static ProductDto ToDto(this Product product)
        {
            var mainImage = product.ProductImages?
                .OrderByDescending(i => i.IsMain)
                .ThenBy(i => i.SortOrder)
                .FirstOrDefault();

            var approvedReviews = product.Reviews
                .Where(r => r.IsApproved)
                .ToList();

            return new ProductDto
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Slug = product.Slug,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                StockQuantity = product.StockQuantity,

                MainImageUrl = mainImage?.ImageUrl,

                AverageRating = approvedReviews.Any()
                    ? Math.Round((decimal)approvedReviews.Average(r => r.Rating), 1) : 0,

                ReviewsCount = approvedReviews.Count,

                Images = product.ProductImages?
                    .Select(i => new ProductImageDto
                    {
                        ProductImageId = i.ProductImageId,
                        ProductId = i.ProductId,
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText,
                        IsMain = i.IsMain,
                        SortOrder = i.SortOrder,
                        CreatedAt = i.CreatedAt
                    })
                    .ToList() ?? new List<ProductImageDto>()
            };
        }


        public static List<ProductDto> ToDtoList(this IEnumerable<Product> product) =>
            product.Select(p => p.ToDto()).ToList();
    }
}
