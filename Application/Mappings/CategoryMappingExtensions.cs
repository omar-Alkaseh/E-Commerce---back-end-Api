using Application.Features.Categories.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public static class CategoryMappingExtensions
    {
        public static CategoryDto MapToDto(this Category category)
        {
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,

                Children = category.InverseParentCategory?
                    .Select(MapToDto)
                    .ToList() ?? new List<CategoryDto>()
            };
        }

        public static List<CategoryDto> MapToDtoList(this IEnumerable<Category> categories) =>
            categories.Select(c => c.MapToDto()).ToList();
    }
}
