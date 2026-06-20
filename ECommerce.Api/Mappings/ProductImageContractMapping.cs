using Application.Features.ProductImages.DTOs;
using Contracts.Responses;

namespace ECommerce.Api.Mappings
{
    public static class ProductImageContractMapping
    {
        public static ProductImageResponse ToResponse(this ProductImageDto image)
        {
            var response = new ProductImageResponse
            {
                ProductImageId = image.ProductImageId,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl,
                AltText = image.AltText,
                IsMain = image.IsMain,
                SortOrder = image.SortOrder,
                CreatedAt = image.CreatedAt,
            };

            return response;
        }
    }
}
