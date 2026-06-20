using Application.Features.Categories.DTOs;
using Contracts.Responses;

namespace ECommerce.Api.Mappings
{
    public static class CategoryContractMapping
    {
        public static CategoryResponse ToResponse(this CategoryDto category)
        {
            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategoryName,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,

                Children = category.Children
                    .Select(child => child.ToResponse())
                    .ToList() ?? new List<CategoryResponse>()
            };
        }

        public static List<CategoryResponse> ToResponseList(this IEnumerable<CategoryDto> categories) =>
            categories.Select(c => c.ToResponse()).ToList();
    }
}
