using Application.Features.Products.DTOs;
using Contracts.Responses;

namespace ECommerce.Api.Mappings
{
    public static class ProductContractMapping
    {
        public static ProductResponse ToResponse(this ProductDto product)
        {
            return new ProductResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Sku = product.Sku,
                Slug = product.Slug,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,

                AverageRating = product.AverageRating ?? 0,
                ReviewsCount = product.ReviewsCount ?? 0,

                MainImageUrl = product.MainImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,

                Images = product.Images.Select(i => new ProductImageResponse
                {
                    ProductImageId = i.ProductImageId,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsMain = i.IsMain,
                    SortOrder = i.SortOrder,
                    CreatedAt = i.CreatedAt
                }).ToList()
            };
        }
    }
}
