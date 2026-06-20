using Application.Features.ProductImages.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public static class ProductImageMappingExtensions
    {
        public static ProductImageDto ToDto(this ProductImage image)
        {
            return new ProductImageDto
            {
                ProductId = image.ProductId,
                ProductImageId = image.ProductImageId,
                ImageUrl = image.ImageUrl,
                AltText = image.AltText,
                IsMain = image.IsMain,
                SortOrder = image.SortOrder,
                CreatedAt = image.CreatedAt
            };
        }

        public static IEnumerable<ProductImageDto> ToDtoList(this IEnumerable<ProductImage> image) =>
            image.Select(i => i.ToDto()).ToList();

    }
}