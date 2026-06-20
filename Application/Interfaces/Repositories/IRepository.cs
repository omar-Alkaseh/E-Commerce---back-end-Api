namespace Application.Interfaces.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entity);
    }
}
