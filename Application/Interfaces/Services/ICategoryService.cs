using Application.Features.Categories.DTOs;

namespace Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<CategoryDto> CreateAsync(CreateCategoryDto request);
        Task<CategoryDto> UpdateAsync(int categoryId, UpdateCategoryDto request);
        Task DeleteAsync(int categoryId);

        Task<CategoryDto> GetByIdAsync(int categoryId);
        Task<IReadOnlyList<CategoryDto>> GetAllAsync();
        Task<IReadOnlyList<CategoryDto>> GetTreeAsync();
    }
}
