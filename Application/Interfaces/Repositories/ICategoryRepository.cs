using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByIdWithParentAsync(int categoryId);
        Task<Category?> GetByIdWithChildrenAsync(int categoryId);

        Task<IReadOnlyList<Category>> GetAllWithChildrenAsync();

        Task<bool> ExistsAsync(int categoryId);
        Task<bool> NameExistsAsync(string name);
        Task<bool> SlugExistsAsync(string slug);

        Task<bool> NameExistsForOtherCategoryAsync(int categoryId, string name);
        Task<bool> SlugExistsForOtherCategoryAsync(int categoryId, string slug);
    }
}
